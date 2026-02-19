using UnityEngine;

namespace _Game.Content.Features.Interfaces
{
    public interface IRenderer<TData, TConfig, TMaterial>
    {
        void Initialize(TData data, TConfig config, TMaterial material, Transform parent = null);
        void Dispose();
    }
}