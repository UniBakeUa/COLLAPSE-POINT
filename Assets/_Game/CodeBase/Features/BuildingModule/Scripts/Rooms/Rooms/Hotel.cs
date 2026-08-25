namespace _Game.CodeBase.Features.BuildingModule.Scripts.Rooms.Rooms
{
    public class Hotel : Room
    {
        public override bool CanAttachOnSide(RoomSide side) => side != RoomSide.Bottom;
    }
}