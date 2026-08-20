using _Game.Core.SceneLoadingModule.Scripts;
using Cysharp.Threading.Tasks;

namespace _Game.Core.GlobalStateMachine.Scripts.States
{
    public class GameplayState : GlobalStateBase
    {
        private const string SceneKey = "Game";
        private readonly AddressableSceneLoader _sceneLoader;

        public GameplayState(AddressableSceneLoader sceneLoader)
        {
            _sceneLoader = sceneLoader;
        }

        public override void Enter()
        {
            LoadAsync().Forget();
        }

        private async UniTask LoadAsync()
        {
            await _sceneLoader.LoadAsync(SceneKey);
        }

        public override void Exit()
        {
            _sceneLoader.UnloadAsync().Forget();
        }
    }
}