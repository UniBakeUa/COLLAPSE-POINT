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
        [SerializeField] private RoomPoolConfig _roomPoolConfig;
        [SerializeField] private HotelPoolConfig _hotelPoolConfig;
        [SerializeField] private SupportsConfig _supportsConfig;
        [SerializeField] private WeightDistributionConfig _weightDistributionConfig;
        [SerializeField] private LayerMask _roomsLayerMask;
        [SerializeField] private LayerMask _supportsLayerMask;
        
        [SerializeField] private Support _supportPrefab;

        public override void InstallBindings()
        {
            Container.DeclareSignal<RoomSpawnedSignal>();
            Container.DeclareSignal<SupportPlacedSignal>(); 
            
            Container.Bind<RoomPoolConfig>().FromInstance(_roomPoolConfig).AsSingle();
            Container.Bind<HotelPoolConfig>().FromInstance(_hotelPoolConfig).AsSingle();

            Container.BindMemoryPool<Support, SupportPool>()
                .WithInitialSize(10)
                .FromComponentInNewPrefab(_supportPrefab)
                .UnderTransformGroup("Supports");
            
            Container.Bind<WeightDistributionSystem>()
                .AsSingle()
                .WithArguments(_weightDistributionConfig, _roomsLayerMask)
                .NonLazy();
            
            Container.BindInterfacesAndSelfTo<RoomWeightInspector>()
                .AsSingle()
                .WithArguments(FindAnyObjectByType<Camera>(), _roomsLayerMask);//FindAnyObjectByType temp
            
            Container.Bind<RoomSpawner>().AsSingle().WithArguments(_supportsLayerMask);

            Container.BindInterfacesAndSelfTo<SupportFactory>().AsSingle().WithArguments(_supportsConfig);
            Container.Bind<SupportsGenerator>().AsSingle().WithArguments(_supportsConfig, _roomsLayerMask);

            Container.Bind<BuilderCrewManager>().AsSingle();
        }
    }
}