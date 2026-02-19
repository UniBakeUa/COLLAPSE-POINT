using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Game.Content.Features.BuildingModule.Scripts.Builders
{
    [CreateAssetMenu(menuName = "Game/Configs/BuildersConfig", fileName = "BuildersConfig")]
    public class BuildersConfig : ScriptableObject
    {
        [Header("Progression")]
        public List<BuilderUpgradeLevel> upgradeLevels;

        public BuilderUpgradeLevel GetLevel(int index) =>
            upgradeLevels[Mathf.Clamp(index, 0, upgradeLevels.Count - 1)];
    }

    [Serializable]
    public class BuilderUpgradeLevel
    {
        public string levelName;
        public int maxTeams;
        public float buildSpeedMultiplier; // 1.0 = normal, 1.5 = 50% faster
        public int upgradeCost;
    }
}