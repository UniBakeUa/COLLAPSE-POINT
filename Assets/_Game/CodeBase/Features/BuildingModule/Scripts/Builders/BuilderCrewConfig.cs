using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Game.CodeBase.Features.BuildingModule.Scripts
{
    [Serializable]
    public class BuilderCrewLevel
    {
        public int Level;
        public double BuildTime;
    }
    [CreateAssetMenu(menuName = "Game/Configs/Features/" + nameof(BuilderCrewConfig), fileName = nameof(BuilderCrewConfig))]
    public class BuilderCrewConfig : ScriptableObject
    {
        public List<BuilderCrewLevel> CrewLevels;
        
        public double GetBuildTime(int level)
        {
            foreach (var l in CrewLevels)
                if (l.Level == level)
                    return l.BuildTime;

            return CrewLevels.Count > 0 ? CrewLevels[0].BuildTime : 1.0;
        }
        
        public int MaxLevel => CrewLevels.Count > 0 ? CrewLevels[^1].Level : 1;
    }
}