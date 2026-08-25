using System;
using _Game.CodeBase.Features.BuildingModule.Scripts.Rooms;
using UnityEngine;

namespace _Game.CodeBase.Features.BuildingModule.Scripts
{
    [RequireComponent(typeof(Room))]
    public class BreakableRoom : MonoBehaviour, IBreakable
    {
        [SerializeField] private float _maxLoad = 100f;

        public float CurrentLoad { get; private set; }
        public bool IsBroken { get; private set; }

        public event Action<BreakableRoom> Broke;

        public void ApplyLoad(float amount)
        {
            if (!enabled || IsBroken) return; // вимкнений компонент = кімната незламна

            CurrentLoad += amount;
            if (CurrentLoad >= _maxLoad)
                Break();
        }

        public void Break()
        {
            if (!enabled || IsBroken) return;

            IsBroken = true;
            Broke?.Invoke(this);
            Destroy(gameObject);
        }

        public void SetBreakable(bool canBreak) => enabled = canBreak;
    }
}