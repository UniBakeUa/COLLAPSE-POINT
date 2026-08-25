using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Game.CodeBase.Features.BuildingModule.Scripts.Data
{
    [CreateAssetMenu(menuName = "Game/Configs/SupportsConfig", fileName = "SupportsConfig")]
    public class SupportsConfig : ScriptableObject
    {
        [Header("Construction Rules")]
        public float mixingPenalty;
        public float horizontalWeightEfficiency;

        [Header("Generation Constraints")]
        public int longSupportRoomCountThreshold = 5;
        
        public float maxHorizontalLengthMult;
        public float maxVerticalDriftRatio; // 1 to 7 ratio
        public float supportEdgeMargin;
        public LayerMask maskToCollide;
        public float minLength;
        public float maxLength;
        public float minAngleDifference;

        public float minHorizontalAngle;
        public float maxHorizontalAngle;
        public float minVerticalAngle;
        public float maxVerticalAngle;

        [Header("Thickness Settings")]
        public float minThickness = 0.1f;
        public float maxThickness = 0.4f;
        
        [Header("Material Progression")]
        public List<SupportMaterialLevel> materialLevels;

        public SupportMaterialLevel GetLevel(int index) =>
            materialLevels[Mathf.Clamp(index, 0, materialLevels.Count - 1)];
    }
}