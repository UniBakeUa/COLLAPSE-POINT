using System;
using System.Threading;
using _Game.CodeBase.Core.TimeControllerModule.Scripts;
using _Game.CodeBase.Features.BuildingModule.Scripts.Rooms;
using _Game.CodeBase.Features.BuildingModule.Scripts.Supports;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Game.CodeBase.Features.BuildingModule.Scripts
{
    public class BuilderCrew : IDisposable
    {
        private readonly SupportsGenerator _supportGenerator;
        private readonly SpeedController _speedController;
        private readonly BuilderCrewConfig _config;

        private CancellationTokenSource _cts;

        public Room AssignedRoom { get; private set; }
        public bool IsWorking { get; private set; }
        public int Level { get; private set; } = 1;

        public BuilderCrew(SupportsGenerator supportGenerator, SpeedController speedController,
            BuilderCrewConfig config, int level = 1)
        {
            _supportGenerator = supportGenerator;
            _speedController = speedController;
            _config = config;
            Level = level;
        }

        public void UpgradeLevel()
        {
            if (Level < _config.MaxLevel)
                Level++;
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
                float delay = GetCurrentBuildInterval();
                float speed = Mathf.Max(_speedController.GetCurrentSpeed(), 0.01f);
                
                await UniTask.Delay(TimeSpan.FromSeconds(delay / speed), cancellationToken: token)
                    .SuppressCancellationThrow();

                if (token.IsCancellationRequested) break;

                if (AssignedRoom != null && AssignedRoom.WeightReceiver != null)
                {
                    _supportGenerator.SpawnRandomSupport(AssignedRoom.WeightReceiver, null);
                }
            }
        }
        
        private float GetCurrentBuildInterval()
        {
            double crewBaseTime = _config.GetBuildTime(Level);

            if (AssignedRoom != null && AssignedRoom.WeightReceiver != null)
            {
                var receiver = AssignedRoom.WeightReceiver;

                float supportBuildTime = _supportGenerator.TryGetNextUpgradeLevel(receiver, out int currentLevel)
                    ? _supportGenerator.GetUpgradeBuildTime(currentLevel)
                    : _supportGenerator.GetNewSupportBuildTime();

                return (float)crewBaseTime * supportBuildTime;
            }

            return (float)crewBaseTime;
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