using System;
using System.Collections.Generic;
using _Game.CodeBase.Core.TimeControllerModule.Scripts;
using _Game.CodeBase.Features.BuildingModule.Scripts.Rooms;
using _Game.CodeBase.Features.BuildingModule.Scripts.RoomsAndObjects;
using _Game.CodeBase.Features.BuildingModule.Scripts.Weight;
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
        private readonly SignalBus _signalBus;
        private readonly RoomPoolConfig _poolConfig;
        private readonly DiContainer _container;
        private readonly LayerMask _supportsLayerMask;

        private float _hotelY;
        private float _hotelX;

        private int _nextRoomId = 1;

        private readonly List<Room> _spawnedRooms = new();
        private readonly List<WeightReceiver> _weightReceivers = new();
        private readonly List<(Room anchorRoom, RoomSide side, float freeScore)> _candidatesBuffer = new();
        private static readonly RoomSide[] _allSides = (RoomSide[])Enum.GetValues(typeof(RoomSide));
        public WeightReceiver FirstReceiver => _weightReceivers.Count > 0 ? _weightReceivers[0] : null;

        private readonly List<Rect> _roomRects = new();
        
        public IReadOnlyList<WeightReceiver> WeightReceivers => _weightReceivers;
        public IReadOnlyList<Room> SpawnedRooms => _spawnedRooms;
        
        
        public RoomSpawner(DiContainer container, RoomPoolConfig poolConfig, LayerMask supportsLayerMask,
            SignalBus signalBus)
        {
            _container = container;
            _poolConfig = poolConfig;
            _supportsLayerMask = supportsLayerMask;
            _signalBus = signalBus;
            RegisterExistingReceivers();
        }

        private void RegisterExistingReceivers()
        {
            var existingReceivers = UnityEngine.Object.FindObjectsByType<WeightReceiver>(FindObjectsSortMode.None);
            foreach (var receiver in existingReceivers)
            {
                if (receiver.GetComponentInParent<Room>() != null) continue;
                if (_weightReceivers.Contains(receiver)) continue;

                _weightReceivers.Add(receiver);
                var size = receiver.Data.Size;
                var pos = receiver.Transform.position;
                _roomRects.Add(new Rect(pos.x - size.x / 2f, pos.y - size.y / 2f, size.x, size.y));
            }
        }

        public Bounds GetTotalBounds()
        {
            if (_spawnedRooms.Count == 0)
                return new Bounds(Vector3.zero, Vector3.zero);

            var firstReceiver = _spawnedRooms[0].WeightReceiver;
            Bounds totalBounds = new Bounds(firstReceiver.Transform.position, firstReceiver.Data.Size);

            for (int i = 1; i < _spawnedRooms.Count; i++)
            {
                var receiver = _spawnedRooms[i].WeightReceiver;
                totalBounds.Encapsulate(new Bounds(receiver.Transform.position, receiver.Data.Size));
            }

            return totalBounds;
        }

        public Room SpawnRoom(Room roomPrefab, float weight, Vector3? fixedPosition = null)
        {
            var roomSize = roomPrefab.WeightReceiver.Data.Size;

            var position = fixedPosition ?? (_spawnedRooms.Count == 0
                ? Vector3.zero
                : GetWeightedPosition(roomSize));

            if (_spawnedRooms.Count == 0)
            {
                _hotelY = position.y;
                _hotelX = position.x;
            }

            var room = _container.InstantiatePrefabForComponent<Room>(roomPrefab, position, Quaternion.identity, null);

            room.WeightReceiver.Data.Id = _nextRoomId++;
            room.WeightReceiver.Data.Position = position;
            room.WeightReceiver.Data.SetBaseWeight(weight);

            _spawnedRooms.Add(room);

            if (!_weightReceivers.Contains(room.WeightReceiver))
                _weightReceivers.Add(room.WeightReceiver);

            _roomRects.Add(new Rect(position.x - roomSize.x / 2f, position.y - roomSize.y / 2f, roomSize.x, roomSize.y));

            _signalBus.Fire(new RoomSpawnedSignal { Room = room });

            return room;
        }

        private Vector3 GetWeightedPosition(Vector2 roomSize)
        {
            _candidatesBuffer.Clear();

            List<SideWeightData> weightsList = _poolConfig?.SideWeights;
            float falloffDistance = _poolConfig != null ? _poolConfig.BelowHotelFalloffDistance : 15f;
            float penaltyNearHotel = _poolConfig != null ? _poolConfig.BelowHotelPenaltyNearHotel : 0.7f;

            foreach (var room in _spawnedRooms)
            {
                var receiver = room.WeightReceiver;

                foreach (RoomSide side in _allSides)
                {
                    if (receiver.TryGetComponent<IAttachmentRules>(out var rules) && !rules.CanAttachOnSide(side))
                        continue;

                    float freeScore = EstimateFreeScore(receiver, side, roomSize);
                    if (freeScore <= 0f) continue;

                    float sideWeightMultiplier = 1f;
                    if (weightsList != null)
                    {
                        var found = weightsList.Find(x => x.Side == side);
                        sideWeightMultiplier = found.Weight / 100f;
                    }

                    if (sideWeightMultiplier <= 0f) continue;

                    float finalScore = freeScore * sideWeightMultiplier;

                    Vector3 resultPos = GetFlushPosition(receiver, side, roomSize, 0f);
                    if (resultPos.y < _hotelY)
                    {
                        float distanceFromHotel = Mathf.Abs(resultPos.x - _hotelX);
                        float t = falloffDistance > 0f ? Mathf.Clamp01(distanceFromHotel / falloffDistance) : 1f;
                        float penaltyMultiplier = Mathf.Lerp(penaltyNearHotel, 1f, t);
                        finalScore *= penaltyMultiplier;
                    }

                    _candidatesBuffer.Add((room, side, finalScore));
                }
            }

            if (_candidatesBuffer.Count == 0)
            {
                Debug.LogWarning("[RoomSpawner] Немає вільного місця на краях з урахуванням ваг, спавню біля першої кімнати.");
                return GetFlushPosition(_spawnedRooms[0].WeightReceiver, RoomSide.Top, roomSize, 0);
            }

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

            int maxSpawnAttempts = _poolConfig != null ? _poolConfig.MaxSpawnAttempts : 30;

            var candidates = GetShuffledFlushCandidates(chosen.anchorRoom.WeightReceiver, chosen.side, roomSize);
            foreach (var offset in candidates)
            {
                var position = GetFlushPosition(chosen.anchorRoom.WeightReceiver, chosen.side, roomSize, offset);
                if (!Overlaps(position, roomSize))
                    return position;
            }

            for (var attempt = 0; attempt < maxSpawnAttempts && candidates.Count > 0; attempt++)
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
            float slideStep = _poolConfig != null ? _poolConfig.SlideStep : 0.5f;

            Vector3 calculatedPosition = side switch
            {
                RoomSide.Top => new Vector3(pos.x + slideOffset, pos.y + size.y / 2f + roomSize.y / 2f, 0f),
                RoomSide.Bottom => new Vector3(pos.x + slideOffset, pos.y - size.y / 2f - roomSize.y / 2f, 0f),
                RoomSide.Right => new Vector3(pos.x + size.x / 2f + roomSize.x / 2f, pos.y + slideOffset, 0f),
                _ => new Vector3(pos.x - size.x / 2f - roomSize.x / 2f, pos.y + slideOffset, 0f),
            };

            return side is RoomSide.Top or RoomSide.Bottom
                ? new Vector3(Mathf.Round(calculatedPosition.x / slideStep) * slideStep, calculatedPosition.y, 0f)
                : new Vector3(calculatedPosition.x, Mathf.Round(calculatedPosition.y / slideStep) * slideStep, 0f);
        }

        private bool Overlaps(Vector3 position, Vector2 size)
        {
            var newRect = new Rect(position.x - size.x / 2f, position.y - size.y / 2f, size.x, size.y);

            for (int i = 0; i < _roomRects.Count; i++)
            {
                if (newRect.Overlaps(_roomRects[i]))
                    return true;
            }

            var overlap = Physics2D.OverlapBox(position, size, 0f, _supportsLayerMask);
            return overlap != null;
        }
    }
}