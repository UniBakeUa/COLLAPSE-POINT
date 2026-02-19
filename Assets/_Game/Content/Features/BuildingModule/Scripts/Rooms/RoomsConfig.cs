using System.Collections.Generic;
using _Game.Content.Features.BuildingModule.Scripts.RoomSetup;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Game.Content.Features.BuildingModule.Scripts
{
    
    [CreateAssetMenu(menuName = "Game/Configs/RoomsConfig", fileName = "RoomsConfig")]
    public class RoomsConfig : ScriptableObject
    {
        [Header("Rooms settings")]
        public double spawnDelay;
        
        public List<RoomMaterialLevel> materialLevels;
        
        [Header("Rooms")]
        public List<RoomSetSO> roomSets;
        public RoomForm GetRandomForm()
        {
            List<RoomForm> available = new();

            foreach (var set in roomSets)
                foreach (var form in set.forms)
                    available.Add(form);

            if (available.Count == 0)
            {
                Debug.LogError("No forms found in RoomSets!");
                return null;
            }

            return available[Random.Range(0, available.Count)];
        }
        public RoomMaterialLevel GetLevel(RoomData.RoomDifficulty difficulty) 
            => materialLevels.Find(l => l.difficulty == difficulty);
    }
    [System.Serializable]
    public struct RoomMaterialLevel
    {
        public RoomData.RoomDifficulty difficulty;
        public float maxIncomingLoad;
        public float weightPerCell;
        public Color stressColor;
    }
}