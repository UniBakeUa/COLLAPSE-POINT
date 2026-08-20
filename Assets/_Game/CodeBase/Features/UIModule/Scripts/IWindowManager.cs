namespace _Game.CodeBase.Features.UIModule.Scripts
{
    public interface IWindowManager
    {
        T OpenMain<T>() where T : UIWindowViewBase;
        T OpenOverlay<T>() where T : UIWindowViewBase;
        void CloseTopOverlay();
        void CloseAllOverlays();
        bool TryCloseTop();

        UIWindowViewBase CurrentMain { get; }
        UIWindowViewBase TopOverlay { get; }
    }
}