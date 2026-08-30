
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

        [Header("Below-Hotel Penalty")]
        [SerializeField] private float _belowHotelPenaltyNearHotel = 0.7f;
        [SerializeField] private float _belowHotelFalloffDistance = 15f;

        [Header("Placement")]
        [SerializeField] private float _slideStep = 0.5f;
        [SerializeField] private int _maxSpawnAttempts = 30;
        [SerializeField] private float _adjacencyTolerance = 0.05f;

        public Room GetRandom() => _roomPrefabs[Random.Range(0, _roomPrefabs.Length)];
        public Room Get(int index) => _roomPrefabs[Mathf.Clamp(index, 0, _roomPrefabs.Length - 1)];
        public int Count => _roomPrefabs.Length;

        public List<SideWeightData> SideWeights => _sideWeights;

        public float BelowHotelPenaltyNearHotel => _belowHotelPenaltyNearHotel;
        public float BelowHotelFalloffDistance => _belowHotelFalloffDistance;
        public float SlideStep => _slideStep;
        public int MaxSpawnAttempts => _maxSpawnAttempts;
        public float AdjacencyTolerance => _adjacencyTolerance;
    }
}