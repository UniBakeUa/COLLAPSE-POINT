using UnityEngine;
using Zenject;

namespace _Game.CodeBase.Features.UIModule.Scripts
{
    [CreateAssetMenu(menuName = "Game/Installers/Features/" + nameof(WindowManagerInstaller),
        fileName = nameof(WindowManagerInstaller))]
    public class WindowManagerInstaller : ScriptableObjectInstaller<WindowManagerInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<WindowManager>().AsSingle().NonLazy();
        }
    }
}