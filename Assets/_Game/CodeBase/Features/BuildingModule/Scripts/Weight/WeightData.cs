using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Game.CodeBase.Features.BuildingModule.Scripts.RoomsAndObjects
{
    [Serializable]
    public class WeightData
    {
        public int Id;
        public Vector2 Size;
        public Vector2 Position;

        [Header("Weight System")] [SerializeField]
        private float _baseWeight = 10f;
        public float NotStabilizedLoad { get; private set; }
        public float ReceivedLoad { get; private set; }
        public float BaseWeight => _baseWeight;
        public float TotalWeight => _baseWeight + ReceivedLoad;
        
        public List<int> AttachedSupportIds = new();
        
        public event Action OnWeightChanged;

        public void SetBaseWeight(float weight)
        {
            _baseWeight = Mathf.Max(0f, weight);
            OnWeightChanged?.Invoke();
        }

        public void SetReceivedLoad(float value)
        {
            float clamped = Mathf.Max(0f, value);
            if (Mathf.Approximately(clamped, ReceivedLoad)) return; 

            ReceivedLoad = clamped;
            OnWeightChanged?.Invoke();
        }
        public void SetNotStabilizedLoad(float value)
        {
            float clamped = Mathf.Max(0f, value);
            if (Mathf.Approximately(clamped, NotStabilizedLoad)) return;
        
            NotStabilizedLoad = clamped;
            OnWeightChanged?.Invoke();
        }
    }
}