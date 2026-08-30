using _Game.Core.InputSystemModule.Scripts;
using _Game.Core.SceneLoadingModule.Scripts;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace _Game.Core.GlobalStateMachine.Scripts.States
{
    public class MainMenuState : GlobalStateBase
    {
        [Inject] InputService inputService;
        private const string SceneKey = "MainMenu";
        private readonly AddressableSceneLoader _sceneLoader;

        public MainMenuState(AddressableSceneLoader sceneLoader)
        {
            _sceneLoader = sceneLoader;
        }

        public override void Enter()
        {
            Debug.Log("Entering MainMenu");
            inputService.SwitchToUI();
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