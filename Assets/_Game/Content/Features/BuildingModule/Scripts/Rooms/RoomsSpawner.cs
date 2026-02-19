using System;
using System.Threading;
using _Game.Content.Features.BuildingModule.Scripts;
using _Game.Content.Features.BuildingModule.Scripts.RoomSetup;
using Cysharp.Threading.Tasks;
using Zenject;
using UnityEngine;

public class RoomsSpawner : IInitializable, IDisposable
{
    public event Action<RoomData> OnRoomGenerated;

    private readonly RoomsConfig _roomsConfig;
    private readonly RoomGenerator _roomGenerator;
    private readonly RoomSetSO _startRoomSet;

    private readonly CancellationTokenSource _cts = new CancellationTokenSource();

    public RoomsSpawner(RoomsConfig roomsConfig, RoomGenerator roomGenerator, RoomSetSO startRoomSet)
    {
        _roomsConfig = roomsConfig;
        _roomGenerator = roomGenerator;
        _startRoomSet = startRoomSet;
    }

    public void Initialize()
    {
        CreateInitialRoom();
        StartSpawning().Forget();
    }

    private void CreateInitialRoom()
    {
        if (_startRoomSet == null || _startRoomSet.forms.Count == 0) return;

        var form = _startRoomSet.forms[UnityEngine.Random.Range(0, _startRoomSet.forms.Count)];
        _roomGenerator.RegisterRoomManually(form, Vector2Int.zero, RoomData.RoomDifficulty.Normal);

        OnRoomGenerated?.Invoke(_roomGenerator.Rooms[0]);
    }

    private async UniTaskVoid StartSpawning()
    {
        await SpawnRoomsLoopAsync(_cts.Token);
    }

    public async UniTask SpawnRoomsLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            bool success = _roomGenerator.ForceSpawnRoom(_roomsConfig.GetRandomForm(), RoomData.RoomDifficulty.Normal);

            if (success)
            {
                var lastRoom = _roomGenerator.Rooms[_roomGenerator.Rooms.Count - 1];
                OnRoomGenerated?.Invoke(lastRoom);

                await UniTask.Delay(TimeSpan.FromSeconds(_roomsConfig.spawnDelay), cancellationToken: token);
            }
            else
            {
                Debug.LogWarning("There is not space enough");
                await UniTask.Delay(1000, cancellationToken: token);
            }
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _roomGenerator?.Dispose();
        _cts.Dispose();
    }
}