using UnityEngine;
using Zenject;

namespace _Game.Core.InputSystemModule.Scripts
{
    [CreateAssetMenu(menuName = "Game/Installers/Core/InputModuleInstaller", fileName = "InputModuleInstaller")]
    public class InputModuleInstaller : ScriptableObjectInstaller<InputModuleInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<InputService>().AsSingle().NonLazy();
        }
    }
}