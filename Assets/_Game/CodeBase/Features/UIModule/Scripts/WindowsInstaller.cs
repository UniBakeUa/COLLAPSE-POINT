using System.Linq;
using UnityEngine;
using Zenject;

namespace _Game.CodeBase.Features.UIModule.Scripts
{
    public class WindowsInstaller : MonoInstaller
    {
        [SerializeField] private Transform _uiRoot;

        public override void InstallBindings()
        {
            var screens = _uiRoot.GetComponentsInChildren<UIWindowViewBase>(includeInactive: true).ToList();
            Container.BindInstance(screens);
        }
    }
}