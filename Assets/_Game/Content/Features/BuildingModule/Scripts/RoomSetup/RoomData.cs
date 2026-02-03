using UnityEngine;

namespace _Game.Content.Features.BuildingModule.Scripts.RoomSetup
{
    public struct RoomData
    {
        public int Id;
        public Vector2Int GridPosition;
        public Vector2Int Size;
        public RoomForm Form;
        public RoomDifficulty Difficulty;
        
        public enum RoomDifficulty
        {
            Normal = 0,
            Reinforced = 1,
            Iron = 2
        }
    }
}