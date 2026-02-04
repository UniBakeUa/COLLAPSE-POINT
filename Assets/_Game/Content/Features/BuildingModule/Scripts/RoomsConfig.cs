using System.Collections.Generic;
using _Game.Content.Features.BuildingModule.Scripts.RoomSetup;
using UnityEngine;

namespace _Game.Content.Features.BuildingModule.Scripts
{
    
    [CreateAssetMenu(menuName = "Game/RoomsConfig", fileName = "RoomsConfig")]
    public class RoomsConfig : ScriptableObject
    {
        public double spawnDelay;
        
        public List<RoomSetSO> roomSets;

        public RoomForm GetRandomForm()
        {
            List<RoomForm> available = new();

            foreach (var set in roomSets)
            {
                foreach (var form in set.forms)
                {
                    available.Add(form);
                }
            }

            if (available.Count == 0)
            {
                Debug.LogError("No forms found in RoomSets!");
                return null;
            }

            return available[Random.Range(0, available.Count)];
        }
    }
}