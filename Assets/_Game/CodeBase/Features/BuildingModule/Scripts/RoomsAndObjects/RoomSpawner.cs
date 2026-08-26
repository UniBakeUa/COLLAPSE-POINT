using System;
using System.Collections.Generic;
using _Game.CodeBase.Features.BuildingModule.Scripts.Rooms;
using _Game.CodeBase.Features.BuildingModule.Scripts.RoomsAndObjects;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace _Game.CodeBase.Features.BuildingModule.Scripts
{
    public enum RoomSide { Top, Bottom, Left, Right }
    
    [Serializable]
    public struct SideWeightData
    {
        public RoomSide Side;
        [Range(0f, 100f)] public float Weight;
    }

    public class RoomSpawner
    {
        private const float SlideStep = 0.5f;

        private const int MaxSpawnAttempts = 30;
        private const int FreeSpaceSampleCount = 8;
        private readonly RoomPoolConfig _poolConfig;
        
        private int _nextRoomId = 1;

        private readonly DiContainer _container;
        private readonly List<Room> _spawnedRooms = new();

        private readonly List<WeightReceiver> _weightReceivers = new();

        private readonly List<(Room anchorRoom, RoomSide side, float freeScore)> _candidatesBuffer = new();
        private static readonly RoomSide[] _allSides = (RoomSide[])Enum.GetValues(typeof(RoomSide));

        public IReadOnlyList<Room> SpawnedRooms => _spawnedRooms;

        public WeightReceiver FirstReceiver => _weightReceivers.Count > 0 ? _weightReceivers[0] : null;

        private readonly List<Rect> _roomRects = new();

        public RoomSpawner(DiContainer container, RoomPoolConfig poolConfig)
        {
            _container = container;
            _poolConfig = poolConfig;
            RegisterExistingReceivers();
        }

        private void RegisterExistingReceivers()
        {
            var existingReceivers = UnityEngine.Object.FindObjectsByType<WeightReceiver>(FindObjectsSortMode.None);
            foreach (var receiver in existingReceivers)
            {
                if (receiver.GetComponentInParent<Room>() == null)
                {
                    if (!_weightReceivers.Contains(receiver))
                    {
                        _weightReceivers.Add(receiver);

                        var size = receiver.Data.Size;
                        var pos = receiver.Transform.position;
                        _roomRects.Add(new Rect(pos.x - size.x / 2f, pos.y - size.y / 2f, size.x, size.y));
                    }
                }
            }
        }

        public Room SpawnRoom(Room roomPrefab, float weight, Vector3? fixedPosition = null)
        {
            var roomSize = roomPrefab.WeightReceiver.Data.Size;

            var position = fixedPosition ?? (_spawnedRooms.Count == 0
                ? Vector3.zero
                : GetWeightedPosition(roomSize));

            var room = _container.InstantiatePrefabForComponent<Room>(roomPrefab, position, Quaternion.identity, null);

            room.WeightReceiver.Data.Id = _nextRoomId++;
            room.WeightReceiver.Data.Position = position;
            room.WeightReceiver.Data.SetBaseWeight(weight);

            _spawnedRooms.Add(room);

            if (!_weightReceivers.Contains(room.WeightReceiver))
                _weightReceivers.Add(room.WeightReceiver);

            _roomRects.Add(new Rect(position.x - roomSize.x / 2f, position.y - roomSize.y / 2f, roomSize.x, roomSize.y));

            return room;
        }

        private Vector3 GetWeightedPosition(Vector2 roomSize)
        {
            _candidatesBuffer.Clear();

            // Отримуємо налаштування ваг із конфігу (або ставимо дефолтні, якщо конфіг відсутній)
            List<SideWeightData> weightsList = _poolConfig?.SideWeights;

            foreach (var room in _spawnedRooms)
            {
                var receiver = room.WeightReceiver;
                
                foreach (RoomSide side in _allSides) // Перебираємо всі сторони
                {
                    if (receiver.TryGetComponent<IAttachmentRules>(out var rules) && !rules.CanAttachOnSide(side))
                        continue;

                    float freeScore = EstimateFreeScore(receiver, side, roomSize);
                    if (freeScore <= 0f) continue;

                    // Знаходимо налаштовану вагу для цієї сторони в конфізі
                    float sideWeightMultiplier = 1f;
                    if (weightsList != null)
                    {
                        var found = weightsList.Find(x => x.Side == side);
                        sideWeightMultiplier = found.Weight / 100f; // Переводимо у коефіцієнт
                    }

                    // Якщо вага сторони 0%, взагалі пропускаємо її
                    if (sideWeightMultiplier <= 0f) continue;

                    // Множимо реальну вільність місця на відсоток-вагу з конфігу
                    float finalScore = freeScore * sideWeightMultiplier;
                    _candidatesBuffer.Add((room, side, finalScore));
                }
            }

            if (_candidatesBuffer.Count == 0)
            {
                Debug.LogWarning("[RoomSpawner] Немає вільного місця на краях з урахуванням ваг, спавню біля першої кімнати.");
                return GetFlushPosition(_spawnedRooms[0].WeightReceiver, RoomSide.Top, roomSize, 0);
            }

            // Класична рулетка (Weighted Random) на основі загальної суми балів
            float totalWeight = 0f;
            for (int i = 0; i < _candidatesBuffer.Count; i++)
                totalWeight += _candidatesBuffer[i].freeScore;

            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;

            var chosen = _candidatesBuffer[^1];
            for (int i = 0; i < _candidatesBuffer.Count; i++)
            {
                cumulative += _candidatesBuffer[i].freeScore;
                if (roll <= cumulative)
                {
                    chosen = _candidatesBuffer[i];
                    break;
                }
            }

            var candidates = GetShuffledFlushCandidates(chosen.anchorRoom.WeightReceiver, chosen.side, roomSize);
            foreach (var offset in candidates)
            {
                var position = GetFlushPosition(chosen.anchorRoom.WeightReceiver, chosen.side, roomSize, offset);
                if (!Overlaps(position, roomSize))
                    return position;
            }

            for (var attempt = 0; attempt < MaxSpawnAttempts && candidates.Count > 0; attempt++)
            {
                var offset = candidates[Random.Range(0, candidates.Count)];
                var position = GetFlushPosition(chosen.anchorRoom.WeightReceiver, chosen.side, roomSize, offset);
                if (!Overlaps(position, roomSize))
                    return position;
            }

            return GetFlushPosition(chosen.anchorRoom.WeightReceiver, chosen.side, roomSize,
                candidates.Count > 0 ? candidates[0] : 0f);
        }

        private float EstimateFreeScore(WeightReceiver anchor, RoomSide side, Vector2 roomSize)
        {
            var candidates = GetShuffledFlushCandidates(anchor, side, roomSize);
            if (candidates.Count == 0) return 0f;

            int freeSamples = 0;
            for (int i = 0; i < candidates.Count; i++)
            {
                var testPosition = GetFlushPosition(anchor, side, roomSize, candidates[i]);
                if (!Overlaps(testPosition, roomSize))
                    freeSamples++;
            }

            return (float)freeSamples / candidates.Count;
        }

        /// <summary>
        /// Кандидати зміщення вздовж ребра прилягання (перпендикулярно напрямку side).
        /// Якщо розміри анкера і нової кімнати вздовж цієї осі рівні — єдиний
        /// кандидат offset=0 (ідеальний збіг центрів).
        /// Якщо розміри різні — рівно ДВА кандидати: прилягання до одного або
        /// іншого краю анкера (offset = ±(anchorSize-newDimension)/2), і нічого
        /// між ними — кімната ніколи не "звисає" довільно посередині.
        /// </summary>
        private List<float> GetShuffledFlushCandidates(WeightReceiver anchor, RoomSide side, Vector2 roomSize)
        {
            var size = anchor.Data.Size;
            bool isVertical = side is RoomSide.Top or RoomSide.Bottom;
            float anchorSize = isVertical ? size.x : size.y;
            float newDimension = isVertical ? roomSize.x : roomSize.y;

            float diff = anchorSize - newDimension;

            var candidates = new List<float>();
            if (Mathf.Abs(diff) < 0.001f)
            {
                candidates.Add(0f);
                return candidates;
            }

            float halfDiff = diff / 2f;
            candidates.Add(-halfDiff);
            candidates.Add(halfDiff);

            if (Random.value < 0.5f)
                (candidates[0], candidates[1]) = (candidates[1], candidates[0]);

            return candidates;
        }

        private Vector3 GetFlushPosition(WeightReceiver anchor, RoomSide side, Vector2 roomSize, float slideOffset)
        {
            var pos = anchor.Transform.position;
            var size = anchor.Data.Size;

            Vector3 calculatedPosition = side switch
            {
                RoomSide.Top => new Vector3(pos.x + slideOffset, pos.y + size.y / 2f + roomSize.y / 2f, 0f),
                RoomSide.Bottom => new Vector3(pos.x + slideOffset, pos.y - size.y / 2f - roomSize.y / 2f, 0f),
                RoomSide.Right => new Vector3(pos.x + size.x / 2f + roomSize.x / 2f, pos.y + slideOffset, 0f),
                _ => new Vector3(pos.x - size.x / 2f - roomSize.x / 2f, pos.y + slideOffset, 0f),
            };

            // Вісь дотику лишається точною (half-size + half-size).
            // Вісь ковзання округлюємо до SlideStep — тільки як захист від похибок float.
            return side is RoomSide.Top or RoomSide.Bottom
                ? new Vector3(Mathf.Round(calculatedPosition.x / SlideStep) * SlideStep, calculatedPosition.y, 0f)
                : new Vector3(calculatedPosition.x, Mathf.Round(calculatedPosition.y / SlideStep) * SlideStep, 0f);
        }

        private bool Overlaps(Vector3 position, Vector2 size)
        {
            var newRect = new Rect(position.x - size.x / 2f, position.y - size.y / 2f, size.x, size.y);

            for (int i = 0; i < _roomRects.Count; i++)
            {
                if (newRect.Overlaps(_roomRects[i]))
                    return true;
            }
            return false;
        }
    }
}