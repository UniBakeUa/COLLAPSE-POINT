using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Game.Content.Features.BuildingModule.Scripts
{
    [CreateAssetMenu(menuName = "Game/Configs/SupportsConfig", fileName = "SupportsConfig")]
    public class SupportsConfig : ScriptableObject
    {
        [Header("Construction Rules")]
        public float mixingPenalty;
        public float horizontalWeightEfficiency;

        [Header("Generation Constraints")]
        public float maxHorizontalLengthMult;
        public float maxVerticalDriftRatio; // 1 to 7 ratio
        public float supportEdgeMargin;
        public LayerMask maskToCollide;
        public float minLength;
        public float maxLength;
        public float thickness;
        public float minAngleDifference;
        
        public float minHorizontalAngle;
        public float maxHorizontalAngle;
        public float minVerticalAngle;
        public float maxVerticalAngle;
        
        [Header("Material Progression")] 
        public List<SupportMaterialLevel> materialLevels;
        
        public SupportMaterialLevel GetLevel(int index) =>
            materialLevels[Mathf.Clamp(index, 0, materialLevels.Count - 1)];
    }

    [Serializable]
    public class SupportMaterialLevel
    {
        public string name;
        public float maxLoad;
        public float buildTime;
        public Sprite supportSprite;
        public Color visualColor = Color.white;
        public GameObject supportPrefab; 
    }
}