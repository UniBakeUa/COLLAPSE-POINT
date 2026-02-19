using System;
using System.Collections.Generic;
using _Game.Content.Features.BuildingModule.Scripts.Builders;
using _Game.Content.Features.BuildingModule.Scripts.RoomSetup;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace _Game.Content.Features.BuildingModule.Scripts
{
    public class RoomGenerator : IDisposable
    {
        public IReadOnlyList<RoomData> Rooms => _rooms;
        
        private readonly List<RoomData> _rooms = new();

        [Inject] private RoomGrid _grid;
        
        private readonly RoomSetSO _startRoom;
        private readonly RoomsConfig _roomsConfig;

        private int _nextRoomId = 0;
        private int _minY;
        
        
        private static readonly Vector2Int[] Directions =
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        private static readonly Vector2Int[] WeightedDirections =
        {
            Vector2Int.left, Vector2Int.left, Vector2Int.left,
            Vector2Int.right, Vector2Int.right, Vector2Int.right,
            Vector2Int.up,
            Vector2Int.down
        };

        public RoomGenerator(RoomSetSO startRoom, RoomsConfig roomsConfig)
        {
            _startRoom = startRoom;
            _roomsConfig = roomsConfig;
        }

        public void Dispose()
        {
            _rooms.Clear();
            _nextRoomId = 0;
            _minY = 0;
    
            Debug.Log("RoomGenerator cleared.");
        }

        public bool ForceSpawnRoom(RoomForm form, RoomData.RoomDifficulty difficulty)
        {
            for (int i = 0; i < 30; i++)
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
            sortedRooms.Sort((a, b) => (b.Size.x * b.Size.y).CompareTo(a.Size.x * a.Size.y));

            foreach (var baseRoom in sortedRooms)
            {
                Vector2Int[] shuffledDirs = (Vector2Int[])Directions.Clone();
                Shuffle(shuffledDirs);

                foreach (var dir in shuffledDirs)
                {
                    if (TrySpawnRoom(baseRoom, form, difficulty, dir, out RoomData newRoom, true))
                    {
                        newRoom.Id = _nextRoomId++;
                        RegisterRoom(newRoom);
                        return true;
                    }
                }
            }

            Debug.LogError($"[Generator] Cannot spawn room: {form.formName}");
            return false;
        }

        public bool TrySpawnRoom(RoomData baseRoom, RoomForm form, RoomData.RoomDifficulty difficulty,
            Vector2Int direction, out RoomData newRoom, bool ignoreBias = false)
        {
            newRoom = default;
            Vector2Int formSize = form.GetGridSize();
            List<Vector2Int> myCells = form.GetOccupiedCells(difficulty);

            Vector2Int currentPos = baseRoom.GridPosition;
            if (direction == Vector2Int.up) currentPos += new Vector2Int(0, baseRoom.Size.y);
            else if (direction == Vector2Int.down) currentPos += new Vector2Int(0, -formSize.y);
            else if (direction == Vector2Int.right) currentPos += new Vector2Int(baseRoom.Size.x, 0);
            else if (direction == Vector2Int.left) currentPos += new Vector2Int(-formSize.x, 0);
            int minOverlap = 2;

            if (direction == Vector2Int.up || direction == Vector2Int.down)
            {
                int overlapCells = 2;
                int minX = baseRoom.GridPosition.x - formSize.x + overlapCells;
                int maxX = baseRoom.GridPosition.x + baseRoom.Size.x - overlapCells;

                currentPos.x = Random.Range(minX, maxX + 1);
            }
            else
            {
                int minY = baseRoom.GridPosition.y - formSize.y + minOverlap;
                int maxY = baseRoom.GridPosition.y + baseRoom.Size.y - minOverlap;
                currentPos.y = Random.Range(minY, maxY + 1);
            }

            Vector2Int pushDir = -direction;
            Vector2Int bestValidPos = currentPos;
            bool foundValidAtLeastOnce = false;

            int maxPush = (direction == Vector2Int.up || direction == Vector2Int.down) ? formSize.y : formSize.x;

            for (int step = 0; step <= maxPush; step++)
            {
                Vector2Int testPos = currentPos + (pushDir * step);

                if (testPos.y < _minY) break;

                if (_grid.CanPlace(testPos, myCells))
                {
                    bestValidPos = testPos;
                    foundValidAtLeastOnce = true;
                }
                else
                {
                    break;
                }
            }

            if (!foundValidAtLeastOnce) return false;

            if (!ignoreBias && (direction == Vector2Int.up || direction == Vector2Int.down))
            {
                float currentHeight = Mathf.Abs(bestValidPos.y);
                float currentWidth = Mathf.Abs(bestValidPos.x);

                if (currentHeight > (currentWidth / 2f) + 3)
                {
                    if (Random.value < 0.7f) return false;
                }
            }

            newRoom = new RoomData
            {
                Id = -1,
                GridPosition = bestValidPos,
                Size = formSize,
                Form = form,
                Difficulty = difficulty
            };

            return true;
        }

        public void RegisterRoomManually(RoomForm form, Vector2Int position, RoomData.RoomDifficulty difficulty)
        {
            var room = new RoomData
            {
                Id = _nextRoomId++,
                GridPosition = position,
                Size = form.GetGridSize(),
                Form = form,
                Difficulty = difficulty
            };

            if (_rooms.Count == 0)
                _minY = form.GetGridSize().y / 3;


            RegisterRoom(room);
        }

        [Inject] private BuildersManager managerBUILDERSTEST;

        private void RegisterRoom(RoomData room)
        {
            _rooms.Add(room);
            List<Vector2Int> occupiedCells = room.Form.GetOccupiedCells(room.Difficulty);

            foreach (var offset in occupiedCells)
            {
                _grid.Occupy(room.GridPosition + offset, room.Id);
            }

            managerBUILDERSTEST.BuildForRoom(room, GetNeighbors(room));
        }

        public void RemoveRoom(RoomData room)
        {
            // 1. Видаляємо всі підпорки кімнати з сітки за їх списком ID
            foreach (int supportId in room.AttachedSupportIds)
            {
                _grid.RemoveAllCellsByOwnerId(supportId);
            }

            // 2. Видаляємо саму кімнату з сітки
            _grid.RemoveAllCellsByOwnerId(room.Id);

            // 3. Очищаємо список у самому об'єкті
            room.ClearSupports();
        }

        public List<RoomData> GetNeighbors(RoomData room)
        {
            List<RoomData> neighbors = new List<RoomData>();

            int xStart = Mathf.FloorToInt(room.GridPosition.x);
            int xEnd = xStart + Mathf.FloorToInt(room.Size.x);

            int yBottom = Mathf.FloorToInt(room.GridPosition.y) - 1;
            int yTop = Mathf.FloorToInt(room.GridPosition.y) + Mathf.FloorToInt(room.Size.y);

            for (int x = xStart; x < xEnd; x++)
            {
                CheckAndAdd(new Vector2Int(x, yBottom), neighbors, room.Id);
                CheckAndAdd(new Vector2Int(x, yTop), neighbors, room.Id);
            }

            return neighbors;
        }

        private void CheckAndAdd(Vector2Int pos, List<RoomData> neighbors, int selfId)
        {
            var cell = _grid.GetCell(pos);

            if ((cell?.Type == RoomGrid.CellType.Room) && cell?.OwnerId != selfId)
            {
                var neighbor = _rooms.Find(r => r.Id == cell?.OwnerId);

                if (neighbor != null && !neighbors.Contains(neighbor))
                {
                    neighbors.Add(neighbor);
                }
            }
        }

        private RoomData GetWeightedRandomBaseRoom()
        {
            if (_rooms.Count == 0) return default;
            float totalWeight = 0;
            foreach (var room in _rooms) totalWeight += (room.Size.x * room.Size.y);

            float randomPoint = Random.value * totalWeight;
            float currentWeightSum = 0;

            foreach (var room in _rooms)
            {
                currentWeightSum += (room.Size.x * room.Size.y);
                if (randomPoint <= currentWeightSum) return room;
            }

            return _rooms[_rooms.Count - 1];
        }

        private void Shuffle<T>(T[] array)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
        }
    }
}