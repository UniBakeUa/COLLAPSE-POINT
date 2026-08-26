using _Game.CodeBase.Features.BuildingModule.Scripts.Rooms.Rooms;
using UnityEngine;

namespace _Game.CodeBase.Features.BuildingModule.Scripts.Rooms
{
    [CreateAssetMenu(menuName = "Game/Configs/HotelPoolConfig", fileName = "HotelPoolConfig")]
    public class HotelPoolConfig : ScriptableObject
    {
        [SerializeField] private Hotel[] _hotelPrefabs;

        public Hotel GetRandom() => _hotelPrefabs[Random.Range(0, _hotelPrefabs.Length)];
        public Hotel Get(int index) => _hotelPrefabs[Mathf.Clamp(index, 0, _hotelPrefabs.Length - 1)];
        public int Count => _hotelPrefabs.Length;
    }
}