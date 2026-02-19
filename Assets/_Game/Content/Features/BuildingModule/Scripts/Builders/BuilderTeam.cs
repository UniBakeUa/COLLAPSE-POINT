using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace _Game.Content.Features.BuildingModule.Scripts.Builders
{
    public class BuilderTeam
    {
        public bool IsBusy { get; private set; }
        private readonly SupportsGenerator _generator;

        public BuilderTeam(SupportsGenerator generator)
        {
            _generator = generator;
        }

        public async UniTask BuildAsync(Action buildLogic, float duration, CancellationToken ct)
        {
            IsBusy = true;
            try 
            {
                // Тут можна викликати подію "Початок будівництва" для UI
                await UniTask.Delay((int)(duration * 1000), cancellationToken: ct);
                buildLogic?.Invoke();
            }
            finally 
            {
                IsBusy = false;
            }
        }
    }
}