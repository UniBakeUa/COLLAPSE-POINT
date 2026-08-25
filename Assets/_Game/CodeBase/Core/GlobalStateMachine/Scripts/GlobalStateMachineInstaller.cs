using UnityEngine;
using Zenject;

namespace _Game.Core.GlobalStateMachine.Scripts.States
{
    [CreateAssetMenu(menuName = "Game/Installers/Core/" + nameof(GlobalStateMachineInstaller),
        fileName = nameof(GlobalStateMachineInstaller))]
    public class GlobalStateMachineInstaller : ScriptableObjectInstaller<GlobalStateMachineInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<GlobalStateBase>().To<BootstrapState>().AsSingle();
            Container.Bind<GlobalStateBase>().To<MainMenuState>().AsSingle();
            Container.Bind<GlobalStateBase>().To<GameplayState>().AsSingle();
            
            Container.BindInterfacesAndSelfTo<GlobalStateMachine>().AsSingle().NonLazy();
        }
    }
}