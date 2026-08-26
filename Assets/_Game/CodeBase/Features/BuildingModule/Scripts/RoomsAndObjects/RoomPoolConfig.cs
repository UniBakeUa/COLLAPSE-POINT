
using System.Collections.Generic;
using UnityEngine;

namespace _Game.CodeBase.Features.BuildingModule.Scripts.Rooms
{
    
    [CreateAssetMenu(menuName = "Game/Configs/RoomPoolConfig", fileName = "RoomPoolConfig")]
    public class RoomPoolConfig : ScriptableObject
    {
        [SerializeField] private Room[] _roomPrefabs;

        [SerializeField] 
        private List<SideWeightData> _sideWeights = new List<SideWeightData>
        {
            new SideWeightData { Side = RoomSide.Top, Weight = 70f },
            new SideWeightData { Side = RoomSide.Left, Weight = 15f },
            new SideWeightData { Side = RoomSide.Right, Weight = 15f },
            new SideWeightData { Side = RoomSide.Bottom, Weight = 0f }
        };

        public Room GetRandom() => _roomPrefabs[Random.Range(0, _roomPrefabs.Length)];
        public Room Get(int index) => _roomPrefabs[Mathf.Clamp(index, 0, _roomPrefabs.Length - 1)];
        public int Count => _roomPrefabs.Length;
        
        public List<SideWeightData> SideWeights => _sideWeights;
    }
}