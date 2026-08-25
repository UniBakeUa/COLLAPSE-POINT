using _Game.CodeBase.Features.BuildingModule.Scripts.Data;

namespace _Game.CodeBase.Features.BuildingModule.Scripts.Supports
{
    public interface ISupportFactory
    {
        Support Create(SupportData data, SupportMaterialLevel material);
    }
}