using _Game.Core.GlobalStateMachine.Scripts;
using UnityEngine;
using Zenject;

namespace _Game.CodeBase.Core.GlobalStateMachine.Scripts
{
    [CreateAssetMenu(menuName = "Game/Installers/Core/" + nameof(GlobalSignalBusInstaller),
        fileName = nameof(GlobalSignalBusInstaller))]
    public class GlobalSignalBusInstaller : ScriptableObjectInstaller<GlobalSignalBusInstaller>
    {
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);
            
            Container.DeclareSignal<BootstrapCompletedSignal>();
        }
    }
}