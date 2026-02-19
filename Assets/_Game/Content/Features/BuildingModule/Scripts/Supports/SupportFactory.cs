using _Game.Content.Features.BuildingModule.Scripts.Supports;
using _Game.Content.Features.Interfaces;
using UnityEngine;
using Zenject;

namespace _Game.Content.Features.BuildingModule.Scripts
{
    public interface ISupportFactory
    {
        IRenderer<SupportData, SupportsConfig, SupportMaterialLevel> Create(SupportData data, SupportsConfig config, SupportMaterialLevel material);
    }

    public class SupportFactory : ISupportFactory
    {
        private readonly DiContainer _container;
        private readonly SupportsConfig _config;

        public SupportFactory(DiContainer container, SupportsConfig config)
        {
            _container = container;
            _config = config;
        }

        public IRenderer<SupportData, SupportsConfig, SupportMaterialLevel> Create(SupportData data, SupportsConfig config, SupportMaterialLevel material)
        {
            GameObject prefab = material.supportPrefab; 
            
            if (prefab == null)
            {
                Debug.LogError($"No prefab assigned for material: {material.name}");
                return null;
            }
            
            GameObject instance = _container.InstantiatePrefab(prefab);
            
            var renderer = instance.GetComponent<IRenderer<SupportData, SupportsConfig, SupportMaterialLevel>>();
            renderer.Initialize(data, _config, material);
            
            return renderer;
        }
    }
}