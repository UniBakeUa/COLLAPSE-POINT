using UnityEngine;

namespace _Game.CodeBase.Features.BuildingModule.Scripts.RoomsAndObjects.Data
{
    [CreateAssetMenu(menuName = "Game/Configs/WeightDistributionConfig", fileName = "WeightDistributionConfig")]
    public class WeightDistributionConfig : ScriptableObject
    {
        [Header("Distribution Ratios")]
        [Tooltip("Частка ваги, що йде вбік, коли знизу ВЗАГАЛІ немає кімнати (решта — в опори)")]
        [Range(0f, 1f)] public float sideTransferFraction = 0.15f;

        [Header("Detection")]
        [Tooltip("Толеранс для визначення суміжності кімнат (знизу/збоку), у юнітах")]
        public float detectionTolerance = 0.08f;

        [Header("Optimization")]
        [Tooltip("Нижче цього значення вага не розподіляється далі (оптимізація, уникає мікроскопічних залишків)")]
        public float minWeightToDistribute = 0.01f;
    }
}