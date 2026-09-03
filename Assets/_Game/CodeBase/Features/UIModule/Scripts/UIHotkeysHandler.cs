using _Game.CodeBase.Features.UIModule.Scripts.Windows.Game;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace _Game.CodeBase.Features.UIModule.Scripts
{
    public class UIHotkeysHandler : MonoBehaviour
    {
        [SerializeField] private Key overviewKey = Key.Q;
        [SerializeField] private Key upgradesKey = Key.E;

        private IWindowManager _windowManager;

        [Inject]
        private void Construct(IWindowManager windowManager)
        {
            _windowManager = windowManager;
        }

        private void Update()
        {
            if (Keyboard.current == null) return;

            if (Keyboard.current[overviewKey].wasPressedThisFrame)
            {
                _windowManager.Toggle<OverviewWindow>();
            }

            if (Keyboard.current[upgradesKey].wasPressedThisFrame)
            {
                _windowManager.Toggle<UpgradesWindow>();
            }
        }
    }
}