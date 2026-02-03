using UnityEngine;
using Zenject;

namespace _Game.Core.InputSystemModule.Scripts
{
    [CreateAssetMenu(menuName = "Game/InputModuleInstaller", fileName = "InputModuleInstaller")]
    public class InputModuleInstaller : ScriptableObjectInstaller<InputModuleInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<InputManager>()
                .AsSingle()
                .NonLazy();
        }
    }
}