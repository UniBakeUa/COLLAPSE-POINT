using UnityEngine;
using Zenject;

namespace _Game.CodeBase.Features.BuildingModule.Scripts.Weight
{
    public class WeightDebugDrawer : MonoBehaviour
    {
        private WeightDistributionSystem _weightSystem;

        [Inject]
        private void Construct(WeightDistributionSystem weightSystem)
        {
            _weightSystem = weightSystem;
        }

        private void Update()
        {
            if (_weightSystem?.ActiveFlows == null) return;

            foreach (var flow in _weightSystem.ActiveFlows)
            {
                Debug.DrawLine(flow.FromPos, flow.ToPos, flow.Color);
            }
        }
    }
}