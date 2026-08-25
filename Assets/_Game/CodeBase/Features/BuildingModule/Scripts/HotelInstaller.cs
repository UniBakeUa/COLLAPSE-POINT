using _Game.CodeBase.Features.BuildingModule.Scripts.Data;
using _Game.CodeBase.Features.BuildingModule.Scripts.Rooms;
using _Game.CodeBase.Features.BuildingModule.Scripts.Supports;
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
        [SerializeField] private LayerMask _roomsLayerMask;

        [SerializeField] private Room _roomPrefab;
        [SerializeField] private Support _supportPrefab;

        public override void InstallBindings()
        {
            Container.Bind<RoomPoolConfig>().FromInstance(_roomPoolConfig).AsSingle();
            Container.Bind<HotelPoolConfig>().FromInstance(_hotelPoolConfig).AsSingle();
            
            Container.Bind<Room>().FromInstance(_roomPrefab).AsCached();

            Container.BindMemoryPool<Support, SupportPool>()
                .WithInitialSize(10)
                .FromComponentInNewPrefab(_supportPrefab)
                .UnderTransformGroup("Supports");
            
            Container.Bind<RoomSpawner>().AsSingle();

            Container.BindInterfacesAndSelfTo<SupportFactory>().AsSingle().WithArguments(_supportsConfig);
            Container.Bind<SupportsGenerator>().AsSingle().WithArguments(_supportsConfig, _roomsLayerMask);

            Container.Bind<BuilderCrewManager>().AsSingle();
        }
    }
}