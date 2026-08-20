using UnityEngine;

namespace _Game.CodeBase.Features.UIModule.Scripts
{
    public abstract class UIWindowViewBase : MonoBehaviour
    {
        public virtual void Show()
        {
            gameObject.SetActive(true);
            OnShown();
        }

        public virtual void Hide()
        {
            OnHidden();
            gameObject.SetActive(false);
        }

        protected virtual void OnShown() { }
        protected virtual void OnHidden() { }
    }
}