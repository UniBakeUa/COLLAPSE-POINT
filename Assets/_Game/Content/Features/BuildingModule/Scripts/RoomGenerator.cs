using System;
using System.Collections.Generic;
using _Game.Content.Features.BuildingModule.Scripts.RoomSetup;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace _Game.Content.Features.BuildingModule.Scripts
{
    public class RoomGenerator : IInitializable, IDisposable
    {
        public IReadOnlyList<RoomData> Rooms => _rooms;

        private readonly RoomGrid _grid = new();
        private readonly List<RoomData> _rooms = new();

        private readonly RoomSetSO _startRoom;
        private readonly RoomsConfig _roomsConfig;

        private int _nextRoomId = 0;

        private int _minY;

        private static readonly Vector2Int[] WeightedDirections =
        {
            Vector2Int.left, Vector2Int.left,
            Vector2Int.right, Vector2Int.right,
            Vector2Int.up,
            Vector2Int.down
        };

        public RoomGenerator(RoomSetSO startRoom, RoomsConfig roomsConfig)
        {
            _startRoom = startRoom;
            _roomsConfig = roomsConfig;
        }

        public void Initialize()
        {
            CreateStartRoom(_startRoom);
            SpawnStartRooms();
        }

        public void Dispose()
        {
            //rooms DESTROY
            _rooms.Clear();
        }

        public bool ForceSpawnRoom(RoomForm form, RoomData.RoomDifficulty difficulty)
        {
            // Розрахунок ваг та спроби спавну
            for (int i = 0; i < 50; i++)
            {
                RoomData weightedBase = GetWeightedRandomBaseRoom();
                Vector2Int randomDir = WeightedDirections[Random.Range(0, WeightedDirections.Length)];

                if (TrySpawnRoom(weightedBase, form, difficulty, randomDir, out RoomData newRoom))
                {
                    newRoom.Id = _nextRoomId++;
                    RegisterRoom(newRoom);
                    return true;
                }
            }

            List<RoomData> sortedRooms = new List<RoomData>(_rooms);

            sortedRooms.Sort((a, b) => {
                int areaA = a.Size.x * a.Size.y;
                int areaB = b.Size.x * b.Size.y;
    
                if (areaA != areaB) return areaB.CompareTo(areaA);
                
                return Mathf.Abs(a.GridPosition.y).CompareTo(Mathf.Abs(b.GridPosition.y));
            });

            foreach (var baseRoom in sortedRooms)
            {
                foreach (var dir in WeightedDirections)
                {
                    if (TrySpawnRoom(baseRoom, form, difficulty, dir, out RoomData newRoom))
                    {
                        newRoom.Id = _nextRoomId++;
                        RegisterRoom(newRoom);

                        return true;
                    }
                }
            }

            Debug.LogError("[Generator] cannot spawn rooms");
            return false;
        }

        private void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        public bool TrySpawnRoom(RoomData baseRoom, RoomForm form, RoomData.RoomDifficulty difficulty,
            Vector2Int direction, out RoomData newRoom)
        {
            newRoom = default;
            Vector2Int formSize = form.GetGridSize(difficulty);
            Vector2Int spawnPos = baseRoom.GridPosition;
            
            if (direction == Vector2Int.up) spawnPos += new Vector2Int(0, baseRoom.Size.y);
            else if (direction == Vector2Int.down) spawnPos += new Vector2Int(0, -formSize.y);
            else if (direction == Vector2Int.right) spawnPos += new Vector2Int(baseRoom.Size.x, 0);
            else if (direction == Vector2Int.left) spawnPos += new Vector2Int(-formSize.x, 0);
            
            if (spawnPos.y < _minY) return false;
            
            float currentHeight = Mathf.Abs(spawnPos.y);
            float currentWidth = Mathf.Abs(spawnPos.x);
            
            if (direction == Vector2Int.up || direction == Vector2Int.down)
            {
                if (currentHeight > (currentWidth / 3f) + 2)
                {
                    if (Random.value < 0.8f) return false;
                }
            }
            
            for (int x = 0; x < formSize.x; x++)
            {
                for (int y = 0; y < formSize.y; y++)
                {
                    Vector2Int cellToCheck = spawnPos + new Vector2Int(x, y);
                    if (!_grid.IsFree(cellToCheck))
                    {
                        return false;
                    }
                }
            }
            
            newRoom = new RoomData
            {
                Id = -1, 
                GridPosition = spawnPos,
                Size = formSize,
                Form = form,
                Difficulty = difficulty
            };

            return true;
        }

        private RoomData CreateStartRoom(RoomSetSO startRoom)
        {
            RoomForm roomForm = startRoom.forms[Random.Range(0, startRoom.forms.Count)];

            var room = new RoomData
            {
                Id = _nextRoomId++,
                GridPosition = Vector2Int.zero,
                Size = roomForm.GetGridSize(RoomData.RoomDifficulty.Normal),
                Form = roomForm
            };

            _minY = room.Size.y / 3;

            RegisterRoom(room);
            return room;
        }

        private void SpawnStartRooms()
        {
            for (int i = 0; i < 50; i++)
            {
                RoomForm randomForm = _roomsConfig.GetRandomForm();

                RoomData.RoomDifficulty startDifficulty = RoomData.RoomDifficulty.Normal;

                ForceSpawnRoom(randomForm, startDifficulty);
            }
        }

        private void RegisterRoom(RoomData room)
        {
            _rooms.Add(room);

            for (int x = 0; x < room.Size.x; x++)
            for (int y = 0; y < room.Size.y; y++)
                _grid.Occupy(room.GridPosition + new Vector2Int(x, y), room.Id);
        }

        private RoomData GetWeightedRandomBaseRoom()
        {
            if (_rooms.Count == 0) return default;

            float totalWeight = 0;
            foreach (var room in _rooms)
            {
                totalWeight += (room.Size.x * room.Size.y);
            }

            float randomPoint = Random.value * totalWeight;

            float currentWeightSum = 0;
            foreach (var room in _rooms)
            {
                currentWeightSum += (room.Size.x * room.Size.y);
                if (randomPoint <= currentWeightSum)
                {
                    return room;
                }
            }

            return _rooms[_rooms.Count - 1];
        }
    }
}