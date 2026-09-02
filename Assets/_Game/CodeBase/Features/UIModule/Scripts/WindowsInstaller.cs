using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace _Game.CodeBase.Features.UIModule.Scripts
{
    public class WindowsInstaller : MonoInstaller
    {
        [SerializeField] private List<Transform> _uiRoot;

        public override void InstallBindings()
        {
            var screens = new List<UIWindowViewBase>();
            foreach (var uiRoot in _uiRoot)
            {
                 screens = uiRoot.GetComponentsInChildren<UIWindowViewBase>(includeInactive: true).ToList();
            }
            
            Container.BindInstance(screens);
        }
    }
}