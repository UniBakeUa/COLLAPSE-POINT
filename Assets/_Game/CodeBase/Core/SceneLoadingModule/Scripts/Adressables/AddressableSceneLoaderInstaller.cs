using UnityEngine;
using Zenject;

namespace _Game.Core.SceneLoadingModule.Scripts
{
    [CreateAssetMenu(menuName = "Game/Installers/Core/" + nameof(AddressableSceneLoaderInstaller),
        fileName = nameof(AddressableSceneLoaderInstaller))]
    public class AddressableSceneLoaderInstaller : ScriptableObjectInstaller<AddressableSceneLoaderInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<AddressableSceneLoader>().AsTransient();
        }
    }
}