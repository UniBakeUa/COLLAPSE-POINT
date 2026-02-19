using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using _Game.Content.Features.BuildingModule.Scripts.RoomSetup;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;

namespace _Game.Content.Features.BuildingModule.Scripts.Builders
{
    public class BuildersManager : IDisposable
    {
        private readonly List<BuilderTeam> _teams = new();
        private readonly SupportsGenerator _generator;
        private readonly BuildersConfig _buildersConfig;
        private readonly SupportsConfig _supportsConfig;
        
        private readonly CancellationTokenSource _cts = new();

        private int _currentLevel = 0;

        public BuildersManager(SupportsGenerator generator, BuildersConfig bConfig, SupportsConfig sConfig)
        {
            _generator = generator;
            _buildersConfig = bConfig;
            _supportsConfig = sConfig;
            InitializeTeams();
        }

        private void InitializeTeams()
        {
            var stats = _buildersConfig.GetLevel(_currentLevel);
            for (int i = 0; i < stats.maxTeams; i++)
            {
                _teams.Add(new BuilderTeam(_generator));
            }
        }

        public bool HasFreeTeam => _teams.Any(t => !t.IsBusy);
        
        public void BuildForRoom(RoomData targetRoom, List<RoomData> neighbors)
        {
            // Знаходимо вільну команду TEMPPPPPPPPPPPPPPPPPPPPPP
            var team = _teams.FirstOrDefault(t => !t.IsBusy);
            if (team == null) return;

            int currentCount = targetRoom.AttachedSupportIds.Count;
    
            // Ліміт балок на кімнату (можна винести в конфіг) TEMPPPPPPPPPPPPPPPPPPPPPP
            if (currentCount >= 10) return;

            var stats = _buildersConfig.GetLevel(_currentLevel);
            float duration = _supportsConfig.GetLevel(0).buildTime / stats.buildSpeedMultiplier;

            // FROM UI IN SOON
            bool isHorizontal = Random.value > 0.5f;
            
            team.BuildAsync(() => 
            {
                if (_cts.Token.IsCancellationRequested) return;

                _generator.PlaceSupport(targetRoom, isHorizontal);
                ContinueNextStep(targetRoom, neighbors).Forget();
        
            }, duration, _cts.Token).Forget();
        }

        private async UniTaskVoid ContinueNextStep(RoomData targetRoom, List<RoomData> neighbors)
        {
            if (_cts.Token.IsCancellationRequested) return;

            await UniTask.Yield(PlayerLoopTiming.Update, _cts.Token); 
            
            if (targetRoom == null) return;

            BuildForRoom(targetRoom, neighbors);
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
            _teams.Clear();
        }
    }
}