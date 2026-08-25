using System;
using System.Collections.Generic;
using System.Linq;
using _Game.CodeBase.Features.BuildingModule.Scripts.Rooms;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace _Game.CodeBase.Features.BuildingModule.Scripts
{
    public enum RoomSide { Top, Bottom, Left, Right }

    public class RoomSpawner
    {
        private const float GridUnit = 2f; 

        private float SnapToGrid(float value) => Mathf.Round(value / GridUnit) * GridUnit;
        
        private const int MaxSpawnAttempts = 30;
        private const int FreeSpaceSampleCount = 8;

        private int _nextRoomId = 1;

        private readonly DiContainer _container;
        private readonly List<Room> _spawnedRooms = new();

        private readonly List<(Room room, RoomSide side, float freeScore)> _candidatesBuffer = new();
        private static readonly RoomSide[] _allSides = (RoomSide[])Enum.GetValues(typeof(RoomSide));
        
        public IReadOnlyList<Room> SpawnedRooms => _spawnedRooms;
        public Room FirstRoom => _spawnedRooms.Count > 0 ? _spawnedRooms[0] : null;
        
        private readonly List<Rect> _roomRects = new();
        public RoomSpawner(DiContainer container)
        {
            _container = container;
        }

        public Room SpawnRoom(Room roomPrefab, float weight, Vector3? fixedPosition = null)
        {
            var roomSize = roomPrefab.Size;

            var position = fixedPosition ?? (_spawnedRooms.Count == 0
                ? Vector3.zero
                : GetWeightedPosition(roomSize));

            var room = _container.InstantiatePrefabForComponent<Room>(roomPrefab, position, Quaternion.identity, null);
            room.Initialize(_nextRoomId++, position, weight);

            _spawnedRooms.Add(room);
            _roomRects.Add(new Rect(position.x - roomSize.x / 2f, position.y - roomSize.y / 2f, roomSize.x, roomSize.y));
            return room;
        }

        private Vector3 GetWeightedPosition(Vector2 roomSize)
        {
            _candidatesBuffer.Clear();

            foreach (var room in _spawnedRooms)
            {
                foreach (RoomSide side in _allSides)
                {
                    if (room is IAttachmentRules rules && !rules.CanAttachOnSide(side))
                        continue;

                    float freeScore = EstimateFreeScore(room, side, roomSize);
                    if (freeScore > 0f)
                        _candidatesBuffer.Add((room, side, freeScore));
                }
            }

            if (_candidatesBuffer.Count == 0)
            {
                Debug.LogWarning("[RoomSpawner] Немає вільного місця ніде, спавню з накладенням біля першої кімнати.");
                return GetPointOnSide(FirstRoom, RoomSide.Top, roomSize);
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

            for (var attempt = 0; attempt < MaxSpawnAttempts; attempt++)
            {
                var position = GetPointOnSide(chosen.room, chosen.side, roomSize);
                if (!Overlaps(position, roomSize))
                    return position;
            }

            Debug.LogWarning("[RoomSpawner] Обраний слот виявився зайнятим, спавню з накладенням.");
            return GetPointOnSide(chosen.room, chosen.side, roomSize);
        }

        private float EstimateFreeScore(Room anchor, RoomSide side, Vector2 roomSize)
        {
            int freeSamples = 0;
            for (var i = 0; i < FreeSpaceSampleCount; i++)
            {
                var samplePosition = GetPointOnSide(anchor, side, roomSize);
                if (!Overlaps(samplePosition, roomSize))
                    freeSamples++;
            }
            return (float)freeSamples / FreeSpaceSampleCount;
        }

        private Vector3 GetPointOnSide(Room anchor, RoomSide side, Vector2 roomSize)
        {
            var pos = anchor.transform.position;
            var size = anchor.Data.Size;

            return side switch
            {
                RoomSide.Top => new Vector3(
                    RandomAlongEdge(pos.x, size.x, roomSize.x),
                    pos.y + size.y / 2f + roomSize.y / 2f, 0f),

                RoomSide.Bottom => new Vector3(
                    RandomAlongEdge(pos.x, size.x, roomSize.x),
                    pos.y - size.y / 2f - roomSize.y / 2f, 0f),

                RoomSide.Right => new Vector3(
                    pos.x + size.x / 2f + roomSize.x / 2f,
                    RandomAlongEdge(pos.y, size.y, roomSize.y), 0f),

                _ => new Vector3(
                    pos.x - size.x / 2f - roomSize.x / 2f,
                    RandomAlongEdge(pos.y, size.y, roomSize.y), 0f),
            };
        }

        private float RandomAlongEdge(float anchorCenter, float anchorSize, float newRoomDimension)
        {
            float half = anchorSize / 2f - newRoomDimension / 2f;
            if (half <= 0f) return SnapEdgeToGrid(anchorCenter, newRoomDimension);

            float raw = Random.Range(anchorCenter - half, anchorCenter + half);
            return SnapEdgeToGrid(raw, newRoomDimension);
        }
        private float SnapEdgeToGrid(float center, float dimension)
        {
            float edge = center - dimension / 2f;
            float snappedEdge = Mathf.Round(edge / GridUnit) * GridUnit;
            return snappedEdge + dimension / 2f;
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