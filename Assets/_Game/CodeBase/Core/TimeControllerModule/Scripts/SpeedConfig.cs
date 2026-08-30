using System.Collections.Generic;
using UnityEngine;

namespace _Game.CodeBase.Core.TimeControllerModule.Scripts
{
    [CreateAssetMenu(menuName = "Game/Configs/Core/" + nameof(SpeedConfig), fileName = nameof(SpeedConfig))]
    public class SpeedConfig : ScriptableObject
    {
        public List<int> speedCollection;
    }
}