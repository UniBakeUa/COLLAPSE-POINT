using System;
using System.Threading;
using _Game.CodeBase.Features.BuildingModule.Scripts.Rooms;
using _Game.CodeBase.Features.BuildingModule.Scripts.Supports;
using Cysharp.Threading.Tasks;

namespace _Game.CodeBase.Features.BuildingModule.Scripts
{
    public class BuilderCrew : IDisposable
    {
        private readonly SupportsGenerator _supportGenerator;
        private readonly float _buildIntervalSeconds;

        private CancellationTokenSource _cts;

        public Room AssignedRoom { get; private set; }
        public bool IsWorking { get; private set; }

        public BuilderCrew(SupportsGenerator supportGenerator, float buildIntervalSeconds = 3f)
        {
            _supportGenerator = supportGenerator;
            _buildIntervalSeconds = buildIntervalSeconds;
        }

        public void AssignToRoom(Room room)
        {
            StopWork();

            AssignedRoom = room;
            StartWork();
        }

        private void StartWork()
        {
            if (AssignedRoom == null) return;

            IsWorking = true;
            _cts = new CancellationTokenSource();
            BuildLoop(_cts.Token).Forget();
        }

        private async UniTaskVoid BuildLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_buildIntervalSeconds), cancellationToken: token)
                    .SuppressCancellationThrow();

                if (token.IsCancellationRequested) break;

                _supportGenerator.SpawnRandomSupport(AssignedRoom, null);
            }
        }

        public void StopWork()
        {
            IsWorking = false;
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        public void Dispose()
        {
            StopWork();
        }
    }
}