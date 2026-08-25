namespace _Game.CodeBase.Features.BuildingModule.Scripts.Rooms
{
    public interface IAttachmentRules
    {
        bool CanAttachOnSide(RoomSide side);
    }
}