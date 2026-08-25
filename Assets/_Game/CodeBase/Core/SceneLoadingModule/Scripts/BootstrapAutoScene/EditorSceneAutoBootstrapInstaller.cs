using UnityEngine;
using Zenject;

namespace _Game.CodeBase.Core.SceneLoadingModule.Scripts.BootstrapAutoScene
{
    [CreateAssetMenu(menuName = "Game/Installers/Core/" + nameof(EditorSceneAutoBootstrapInstaller),
        fileName = nameof(EditorSceneAutoBootstrapInstaller))]
    public class EditorSceneAutoBootstrapInstaller : ScriptableObjectInstaller<EditorSceneAutoBootstrapInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<EditorSceneAutoBootstrap>().AsSingle().NonLazy();
        }
    }
}