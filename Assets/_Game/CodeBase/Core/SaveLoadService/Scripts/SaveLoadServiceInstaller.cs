using UnityEngine;
using Zenject;

namespace _Game.CodeBase.Core.SaveLoadService.Scripts
{
    [CreateAssetMenu(menuName = "Game/Installers/Core/" + nameof(SaveLoadServiceInstaller),
        fileName = nameof(SaveLoadServiceInstaller))]
    public class SaveLoadServiceInstaller : ScriptableObjectInstaller<SaveLoadServiceInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<LocalSaveProvider>().AsSingle();
            Container.Bind<CloudSaveProvider>().AsSingle();
            Container.Bind<FileSystemRepository>().AsSingle();
            Container.Bind<ISaveLoadService>().To<SaveLoadService>().AsSingle();
        }
    }
}