namespace _Game.CodeBase.Features.UIModule.Scripts
{
    public abstract class ToggleWindowButtonBase<T> : OpenWindowButtonBase<T> where T : UIWindowViewBase
    {
        protected override void OnClick()
        {
            WindowManager.Toggle<T>();
        }
    }
}