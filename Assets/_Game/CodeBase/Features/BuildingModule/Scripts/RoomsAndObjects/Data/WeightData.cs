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
        
        [Header("Weight System")]
        [SerializeField] private float _baseWeight = 10f;
        public float AdditionalWeight;

        public List<int> AttachedSupportIds = new();

        public float TotalWeight => _baseWeight + AdditionalWeight;

        public void SetBaseWeight(float weight) => _baseWeight = Mathf.Max(0f, weight);
        public void AddWeight(float amount) => AdditionalWeight = Mathf.Max(0f, AdditionalWeight + amount);
    }
}