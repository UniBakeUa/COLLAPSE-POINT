using System.Collections.Generic;
using UnityEngine;

namespace _Game.Content.Features.BuildingModule.Scripts.RoomSetup
{
    [CreateAssetMenu(menuName = "Game/Rooms/RoomSet", fileName = "Default RoomSet")]
    public class RoomSetSO : ScriptableObject
    {
        public List<RoomForm> forms;
        
        private void OnValidate()
        {
            if (forms == null) return;
            foreach (var form in forms)
            {
                form.BakeLayout();
            }
        }
    }
}