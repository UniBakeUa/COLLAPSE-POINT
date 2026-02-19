using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Game.Content.Features.BuildingModule.Scripts
{
    public class RoomGrid : IDisposable
    {
        public enum CellType
        {
            Room,
            Support
        }

        private readonly Dictionary<Vector2Int, CellInfo> _cells = new();

        private readonly Dictionary<int, List<Vector2Int>> _ownerToCells = new();
        
        public IReadOnlyDictionary<Vector2Int, CellInfo> GetAllCells() => _cells;

        public struct CellInfo
        {
            public CellType Type;
            public int OwnerId;
            public float AccumulatedLoad;
            public bool IsVerticalSupport;
        }

        public void Occupy(Vector2Int pos, int roomId)
        {
            _cells[pos] = new CellInfo { Type = CellType.Room, OwnerId = roomId };
        }

       

        public void BlockCell(Vector2Int pos, bool isVertical, int supportId)
        {
            if (_cells.TryGetValue(pos, out var info) && info.Type == CellType.Room) return;

            _cells[pos] = new CellInfo
            {
                Type = CellType.Support,
                OwnerId = supportId,
                IsVerticalSupport = isVertical
            };
        }

        public void OccupyAsSupport(Vector2Int pos, int supportId)
        {
            if (_cells.TryGetValue(pos, out var cell) && cell.Type == CellType.Room)
                return;

            _cells[pos] = new CellInfo { Type = CellType.Support, OwnerId = supportId };
        }

        public bool CanPlace(Vector2Int basePos, IEnumerable<Vector2Int> occupiedOffsets)
        {
            foreach (var offset in occupiedOffsets)
            {
                Vector2Int pos = basePos + offset;
                var cell = GetCell(pos);
                
                if (cell != null) 
                {
                    return false;
                }
            }
            return true;
        }

        public CellInfo? GetCell(Vector2Int pos) =>
            _cells.TryGetValue(pos, out var info) ? info : null;

        public void RemoveAllCellsByOwnerId(int ownerId)
        {
            var keysToRemove = new List<Vector2Int>();

            foreach (var pair in _cells)
            {
                if (pair.Value.OwnerId == ownerId)
                {
                    keysToRemove.Add(pair.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _cells.Remove(key);
            }

            Debug.Log($"[RoomGrid] Removed cells ID: {ownerId}");
        }

        public void ResetLoads()
        {
            var keys = new List<Vector2Int>(_cells.Keys);
            foreach (var key in keys)
            {
                var info = _cells[key];
                info.AccumulatedLoad = 0;
                _cells[key] = info;
            }
        }

        public void AddLoadToOwner(int ownerId, float load)
        {
            if (!_ownerToCells.TryGetValue(ownerId, out var positions)) return;

            foreach (var pos in positions)
            {
                var info = _cells[pos];
                info.AccumulatedLoad += load;
                _cells[pos] = info;
            }
        }

        public void AddLoad(Vector2Int pos, float load)
        {
            if (_cells.TryGetValue(pos, out var info))
            {
                info.AccumulatedLoad += load;
                _cells[pos] = info;
            }
        }

        public CellInfo GetAnyCellByOwnerId(int ownerId)
        {
            foreach (var cell in _cells.Values)
            {
                if (cell.OwnerId == ownerId) return cell;
            }

            return default;
        }
        public void ClearAll()
        {
            _cells.Clear();
            _ownerToCells.Clear();
            Debug.Log("Grid fully cleared.");
        }

        public void Dispose()
        {
            ClearAll();
        }
    }
}