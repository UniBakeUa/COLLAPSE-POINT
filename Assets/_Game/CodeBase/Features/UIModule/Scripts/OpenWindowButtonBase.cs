using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace _Game.CodeBase.Features.UIModule.Scripts
{
    public abstract class OpenWindowButtonBase<T> : MonoBehaviour where T : UIWindowViewBase
    {
        [SerializeField] private Button _button;
        [SerializeField] protected bool _isOverlay;

        protected IWindowManager WindowManager;

        [Inject]
        private void Construct(IWindowManager windowManager)
        {
            WindowManager = windowManager;
        }

        private void Awake()
        {
            _button.onClick.AddListener(OnClick);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnClick);
        }

        protected virtual void OnClick()
        {
            var screen = _isOverlay ? WindowManager.OpenOverlay<T>() : WindowManager.OpenMain<T>();
            OnWindowOpened(screen);
        }

        protected virtual void OnWindowOpened(T screen) { }
    }
}