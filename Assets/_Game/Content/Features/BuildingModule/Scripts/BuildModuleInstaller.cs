using _Game.Content.Features.BuildingModule.Scripts.Builders;
using _Game.Content.Features.BuildingModule.Scripts.RoomSetup;
using UnityEngine;
using Zenject;

namespace _Game.Content.Features.BuildingModule.Scripts
{
    [CreateAssetMenu(menuName = "Game/RoomModuleInstaller", fileName = "RoomModuleInstaller")]
    public class BuildModuleInstaller : ScriptableObjectInstaller<BuildModuleInstaller>
    {
        public RoomSetSO startRoomSO;
        public RoomsConfig roomsConfig;
        public SupportsConfig supportsConfig;
        public BuildersConfig buildersConfig;
        public override void InstallBindings()
        {
            Container.BindInstance(startRoomSO).AsSingle();
            Container.BindInstance(roomsConfig).AsSingle();
            Container.BindInstance(supportsConfig).AsSingle();
            Container.BindInstance(buildersConfig).AsSingle();
            
            Container.BindInterfacesAndSelfTo<RoomGrid>()
                .AsSingle();

            Container.Bind<ISupportFactory>()
                .To<SupportFactory>()
                .AsSingle();
            
            Container.BindInterfacesAndSelfTo<RoomGenerator>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<SupportsGenerator>()
                .AsSingle()
                .NonLazy();
            
            Container.BindInterfacesAndSelfTo<RoomsSpawner>()
                .AsSingle()
                .NonLazy();
            
            Container.BindInterfacesAndSelfTo<BuildersManager>()
                .AsSingle()
                .NonLazy();
        }
    }
}