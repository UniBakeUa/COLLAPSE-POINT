    using _Game.Content.Features.BuildingModule.Scripts.RoomSetup;
    using UnityEngine;
    using Zenject;

    namespace _Game.Content.Features.BuildingModule.Scripts
    {
        [CreateAssetMenu(menuName = "Game/RoomModuleInstaller", fileName = "RoomModuleInstaller")]
        public class RoomModuleInstaller : ScriptableObjectInstaller<RoomModuleInstaller>
        {
            public RoomSetSO startRoomSO;
            public RoomsConfig roomsConfig;
            
            public override void InstallBindings()
            {
                Container.BindInstance(startRoomSO).AsSingle();
                Container.BindInstance(roomsConfig).AsSingle();
                
                Container.BindInterfacesAndSelfTo<RoomGenerator>()
                    .AsSingle()
                    .NonLazy();
            }
        }
    }