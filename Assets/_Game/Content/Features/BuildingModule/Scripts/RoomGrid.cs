using System.Collections.Generic;
using UnityEngine;

namespace _Game.Content.Features.BuildingModule.Scripts
{
    public class RoomGrid
    {
        private readonly Dictionary<Vector2Int, int> _cells;

        public RoomGrid()
        {
            _cells = new Dictionary<Vector2Int, int>();
        }

        public void Occupy(Vector2Int pos, int roomId)
        {
            _cells[pos] = roomId;
        }

        public bool IsFree(Vector2Int pos)
        {
            return !_cells.ContainsKey(pos);
        }
    }

}