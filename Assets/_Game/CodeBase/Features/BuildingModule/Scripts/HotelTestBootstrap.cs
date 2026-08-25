using _Game.CodeBase.Features.BuildingModule.Scripts.Rooms;
using UnityEngine;
using Zenject;

namespace _Game.CodeBase.Features.BuildingModule.Scripts
{
    public class HotelTestBootstrap : MonoBehaviour
    {
        private RoomSpawner _roomSpawner;
        private BuilderCrewManager _crewManager;
        private RoomPoolConfig _roomPoolConfig;
        private HotelPoolConfig _hotelPoolConfig;

        [Inject]
        private void Construct(RoomSpawner roomSpawner, BuilderCrewManager crewManager,
            RoomPoolConfig roomPoolConfig, HotelPoolConfig hotelPoolConfig)
        {
            _roomSpawner = roomSpawner;
            _crewManager = crewManager;
            _roomPoolConfig = roomPoolConfig;
            _hotelPoolConfig = hotelPoolConfig;
        }

        private void Start()
        {
            var hotelPrefab = _hotelPoolConfig.GetRandom();
            var hotelRoom = _roomSpawner.SpawnRoom(hotelPrefab, weight: 100f, fixedPosition: Vector3.zero);

            var breakable = hotelRoom.GetComponent<BreakableRoom>();
            if (breakable != null)
                breakable.SetBreakable(false);

            for (int i = 0; i < 50; i++)
            {
                var prefab = _roomPoolConfig.GetRandom();
                var room = _roomSpawner.SpawnRoom(prefab, Random.Range(1f, 10f));

                var crew = _crewManager.HireCrew();
                _crewManager.AssignCrewToRoom(crew, room);
            }
        }
    }
}