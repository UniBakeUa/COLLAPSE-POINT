using _Game.CodeBase.Features.BuildingModule.Scripts.Data;
using _Game.CodeBase.Features.BuildingModule.Scripts.Weight;

namespace _Game.CodeBase.Features.BuildingModule.Scripts.Supports
{
    public interface ISupportFactory
    {
        Support Create(SupportData data, SupportMaterialLevel material, WeightReceiver targetReceiver);
    }
}