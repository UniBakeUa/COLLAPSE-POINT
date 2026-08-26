using _Game.CodeBase.Features.BuildingModule.Scripts.RoomsAndObjects;
using UnityEngine;

namespace _Game.CodeBase.Features.BuildingModule.Scripts.Rooms
{
    
    [RequireComponent(typeof(WeightReceiver))]
    public class Room : MonoBehaviour
    {
        [field: SerializeField] public WeightReceiver WeightReceiver { get; private set; }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (WeightReceiver == null)
                WeightReceiver = GetComponent<WeightReceiver>();
        }
#endif
    }
}