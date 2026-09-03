using System;
using UnityEngine;

namespace _Game.Core.InputSystemModule.Scripts
{
    public interface IInputService
    {
        event Action<Vector2> OnSimpleClick;
        event Action<Vector2> OnDragStart;
        event Action<Vector2> OnDragging;
        event Action<float> OnScroll;
        event Action OnDragEnd;
        event Action OnLayoutChange;
        
        InputService.Layout CurrentLayout { get; }

        void SwitchToGameplay();
        void SwitchToUI();
        Vector2 GetPointerPosition();
    }
}