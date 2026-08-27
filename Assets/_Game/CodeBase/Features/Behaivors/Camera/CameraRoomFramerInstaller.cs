using Zenject;

namespace _Game.CodeBase.Features.Behaivors
{
    public class CameraRoomFramerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<CameraRoomFramer>().FromComponentInHierarchy().AsSingle();
        }
    }
}