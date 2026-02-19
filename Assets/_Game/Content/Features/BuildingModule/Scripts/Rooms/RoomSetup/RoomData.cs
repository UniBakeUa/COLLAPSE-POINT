using System.Collections.Generic;
using UnityEngine;

namespace _Game.Content.Features.BuildingModule.Scripts.RoomSetup
{
    public class RoomData
    {
        public int Id { get; set; }
        public Vector2Int GridPosition { get; set; }
        public Vector2Int Size { get; set; }
        public RoomForm Form { get; set; }
        public RoomDifficulty Difficulty { get; set; }
        public Transform RoomTransform;
        
        public float SelfWeight { get; set; }
        public float ReceivedLoad{ get; set; }
        public float TotalWeight => SelfWeight + ReceivedLoad;

        public List<int> AttachedSupportIds = new();

        public enum RoomDifficulty
        {
            Normal = 0,
            Reinforced = 1,
            Iron = 2
        }
        public void AddSupport(int id) => AttachedSupportIds.Add(id);
        public void ClearSupports() => AttachedSupportIds.Clear();
    }
}