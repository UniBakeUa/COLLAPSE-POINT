using _Game.CodeBase.Features.BuildingModule.Scripts.Data;
using _Game.CodeBase.Features.BuildingModule.Scripts.Rooms;
using _Game.CodeBase.Features.BuildingModule.Scripts.RoomsAndObjects;
using _Game.CodeBase.Features.BuildingModule.Scripts.RoomsAndObjects.Data;
using _Game.CodeBase.Features.BuildingModule.Scripts.Supports;
using _Game.CodeBase.Features.BuildingModule.Scripts.UI;
using _Game.CodeBase.Features.BuildingModule.Scripts.Weight;
using UnityEngine;
using Zenject;

namespace _Game.CodeBase.Features.BuildingModule.Scripts
{
    [CreateAssetMenu(menuName = "Game/Installers/Features/" + nameof(HotelInstaller), fileName = nameof(HotelInstaller))]
    public class HotelInstaller : ScriptableObjectInstaller<HotelInstaller>
    {
        [SerializeField] private RoomPoolConfig roomPoolConfig;
        [SerializeField] private HotelPoolConfig hotelPoolConfig;
        [SerializeField] private SupportsConfig supportsConfig;
        [SerializeField] private BuilderCrewConfig buildersConfig;
        [SerializeField] private WeightDistributionConfig weightDistributionConfig;
        [SerializeField] private LayerMask roomsLayerMask;
        [SerializeField] private LayerMask supportsLayerMask;
        
        [SerializeField] private Support supportPrefab;

        public override void InstallBindings()
        {
            Container.DeclareSignal<RoomSpawnedSignal>();
            Container.DeclareSignal<SupportPlacedSignal>(); 
            
            Container.Bind<RoomPoolConfig>().FromInstance(roomPoolConfig).AsSingle();
            Container.Bind<HotelPoolConfig>().FromInstance(hotelPoolConfig).AsSingle();

            Container.BindMemoryPool<Support, SupportPool>()
                .WithInitialSize(10)
                .FromComponentInNewPrefab(supportPrefab)
                .UnderTransformGroup("Supports");
            
            Container.Bind<WeightDistributionSystem>()
                .AsSingle()
                .WithArguments(weightDistributionConfig, roomsLayerMask)
                .NonLazy();
            
            Container.BindInterfacesAndSelfTo<RoomWeightInspector>()
                .AsSingle()
                .WithArguments(FindAnyObjectByType<Camera>(), roomsLayerMask);//FindAnyObjectByType temp
            
            Container.Bind<RoomSpawner>().AsSingle().WithArguments(supportsLayerMask);

            Container.BindInterfacesAndSelfTo<SupportFactory>().AsSingle().WithArguments(supportsConfig);
            Container.Bind<SupportsGenerator>().AsSingle().WithArguments(supportsConfig, roomsLayerMask);

            Container.Bind<BuilderCrewManager>().AsSingle().WithArguments(buildersConfig);
        }
    }
}