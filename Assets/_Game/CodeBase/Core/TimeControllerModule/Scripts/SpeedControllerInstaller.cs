using UnityEngine;
using Zenject;

namespace _Game.CodeBase.Core.TimeControllerModule.Scripts
{
    [CreateAssetMenu(menuName = "Game/Installers/Core/" + nameof(SpeedControllerInstaller),
        fileName = nameof(SpeedControllerInstaller))]
    public class SpeedControllerInstaller : ScriptableObjectInstaller<SpeedControllerInstaller>
    {
        [SerializeField] private SpeedConfig speedConfig;
        public override void InstallBindings()
        {
            Container.DeclareSignal<SpeedChangedSignal>();
            
            Container.BindInterfacesAndSelfTo<SpeedController>().AsSingle().WithArguments(speedConfig);
        }
    }
}