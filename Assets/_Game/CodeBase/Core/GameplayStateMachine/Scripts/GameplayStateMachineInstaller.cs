using _Game.Core.GameplayStateMachine.Scripts.States;
using UnityEngine;
using Zenject;

namespace _Game.Core.GameplayStateMachine.Scripts
{
    [CreateAssetMenu(menuName = "Game/Installers/Core/" + nameof(GameplayStateMachineInstaller),
        fileName = nameof(GameplayStateMachineInstaller))]
    public class GameplayStateMachineInstaller : ScriptableObjectInstaller<GameplayStateMachineInstaller>
    {
        public override void InstallBindings()
        {
            
            Container.DeclareSignal<GameplayLoadComplitedSignal>();
            Container.Bind<GameplayStateBase>().To<GameplayLoad>().AsSingle();
            Container.Bind<GameplayStateBase>().To<PauseState>().AsSingle();
            Container.Bind<GameplayStateBase>().To<PlayingState>().AsSingle();
                        
            Container.BindInterfacesAndSelfTo<GameplayStateMachine>().AsSingle().NonLazy();
        }
    }
}