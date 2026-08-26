namespace _Game.CodeBase.Features.BuildingModule.Scripts.Rooms
{
    public interface IBreakable
    {
        float CurrentLoad { get; }
        bool IsBroken { get; }

        void ApplyLoad(float amount);
        void Break();
    }
}