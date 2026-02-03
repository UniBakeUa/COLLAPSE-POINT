using UnityEngine;
using Zenject;

namespace _Game.Core.GameManagerModule.Scripts
{
    [CreateAssetMenu(menuName = "Game/GameManagerModuleInstaller", fileName = "GameManagerModuleInstaller")]
    public class GameManagerModuleInstaller : ScriptableObjectInstaller<GameManagerModuleInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<GameManager>()
                .AsSingle()
                .NonLazy();
        }
    }
}