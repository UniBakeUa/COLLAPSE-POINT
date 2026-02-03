    using Zenject;

    namespace _Game.Content.Features.BuildingModule.Scripts.Renderer
    {
        public class RendererInstaller : MonoInstaller
        {
            public override void InstallBindings()
            {
                Container.BindInterfacesAndSelfTo<RoomRenderer>().FromComponentOn(gameObject).AsSingle();
            }
        }
    }