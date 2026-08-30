using System.Threading;
using _Game.CodeBase.Features.Behaivors;
using _Game.CodeBase.Features.BuildingModule.Scripts.Rooms;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace _Game.CodeBase.Features.BuildingModule.Scripts
{
    public class HotelTestBootstrap2 : MonoBehaviour
    {
        [SerializeField] private int _roomsToSpawn = 500;
        [SerializeField] private float _delayBetweenSpawns = 0.2f;

        private RoomSpawner _roomSpawner;
        private BuilderCrewManager _crewManager;
        private RoomPoolConfig _roomPoolConfig;
        private HotelPoolConfig _hotelPoolConfig;
        private CameraRoomFramer _cameraRoomFramer;

        private CancellationTokenSource _cts;

        [Inject]
        private void Construct(RoomSpawner roomSpawner, BuilderCrewManager crewManager,
            RoomPoolConfig roomPoolConfig, HotelPoolConfig hotelPoolConfig, CameraRoomFramer cameraRoomFramer)
        {
            _roomSpawner = roomSpawner;
            _crewManager = crewManager;
            _roomPoolConfig = roomPoolConfig;
            _hotelPoolConfig = hotelPoolConfig;
            _cameraRoomFramer = cameraRoomFramer;
        }

        private void Start()
        {
            _cts = new CancellationTokenSource();
            SpawnRoomsRoutine(_cts.Token).Forget();
        }

        private async UniTaskVoid SpawnRoomsRoutine(CancellationToken token)
        {
            var hotelPrefab = _hotelPoolConfig.GetRandom();
            var hotelRoom = _roomSpawner.SpawnRoom(hotelPrefab, weight: 100f, fixedPosition: Vector3.zero);

            var breakable = hotelRoom.GetComponent<BreakableRoom>();
            if (breakable != null)
                breakable.SetBreakable(false);

            _cameraRoomFramer.SnapToBounds(_roomSpawner.GetTotalBounds());

            for (int i = 0; i < _roomsToSpawn; i++)
            {
                if (token.IsCancellationRequested) return;

                var prefab = _roomPoolConfig.GetRandom();
                var room = _roomSpawner.SpawnRoom(prefab, Random.Range(1f, 10f));

                var crew = _crewManager.HireCrew();
                _crewManager.AssignCrewToRoom(crew, room);

                _cameraRoomFramer.FrameBounds(_roomSpawner.GetTotalBounds());

                await UniTask.Delay(System.TimeSpan.FromSeconds(_delayBetweenSpawns), cancellationToken: token);
            }
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}