using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace _Game.CodeBase.Features.UIModule.Scripts
{
    public class CloseWindowButton : MonoBehaviour
    {
        [SerializeField] private Button _button;

        private IWindowManager _windowManager;
        private UIWindowViewBase _windowView;

        [Inject]
        private void Construct(IWindowManager windowManager)
        {
            _windowManager = windowManager;
        }

        private void Awake()
        {
            _windowView = GetComponentInParent<UIWindowViewBase>();
            _button.onClick.AddListener(OnClick);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            if (_windowView != null)
            {
                _windowManager.CloseMain(_windowView);
            }
        }
    }
}