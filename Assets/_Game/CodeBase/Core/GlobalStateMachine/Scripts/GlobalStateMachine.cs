using System.Collections.Generic;
using System.Linq.Expressions;
using _Game.CodeBase.Core.SceneLoadingModule.Scripts.BootstrapAutoScene;
using _Game.Core.GlobalStateMachine.Scripts.States;
using _Game.Core.StateMachineModule.Scripts;
using Zenject;

namespace _Game.Core.GlobalStateMachine.Scripts
{
    public class GlobalStateMachine : StateMachineBehaviour<GlobalStateBase>, IInitializable
    {
        private readonly SignalBus _signalBus;
        public GlobalStateMachine(List<GlobalStateBase> states, SignalBus signalBus)
        {
            SetStates(states);
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<BootstrapCompletedSignal>(OnBootstrapCompleted);
            Enter<BootstrapState>();
        }
        private void OnBootstrapCompleted()
        {
            var rememberedScene = EditorSceneAutoBootstrap.GetLastScene();

            switch (rememberedScene)
            {
                case EditorSceneAutoBootstrap.MenuSceneName:
                    Enter<MainMenuState>();
                    break;
                case EditorSceneAutoBootstrap.GameSceneName:
                    Enter<GameplayState>();
                    break;
                default:
                    Enter<MainMenuState>();
                    break;
            }
        }

        public void EnterMainMenu()
        {
            Enter<MainMenuState>();
        }
        public void EnterPlaying()
        {
            Enter<GameplayState>();
        }

        public new void Dispose()
        {
            _signalBus.Unsubscribe<BootstrapCompletedSignal>(EnterMainMenu);
            base.Dispose();
        }
    }
}