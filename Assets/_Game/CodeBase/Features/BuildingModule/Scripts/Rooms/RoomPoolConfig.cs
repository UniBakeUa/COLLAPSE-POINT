using UnityEngine;

namespace _Game.CodeBase.Features.BuildingModule.Scripts.Rooms
{
    [CreateAssetMenu(menuName = "Game/Configs/RoomPoolConfig", fileName = "RoomPoolConfig")]
    public class RoomPoolConfig : ScriptableObject
    {
        [SerializeField] private Room[] _roomPrefabs;

        public Room GetRandom() => _roomPrefabs[Random.Range(0, _roomPrefabs.Length)];
        public Room Get(int index) => _roomPrefabs[Mathf.Clamp(index, 0, _roomPrefabs.Length - 1)];
        public int Count => _roomPrefabs.Length;
    }
}