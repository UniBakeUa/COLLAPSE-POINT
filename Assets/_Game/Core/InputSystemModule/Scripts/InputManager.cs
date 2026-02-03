using System;
using UnityEngine;
using Zenject;

namespace _Game.Core.InputSystemModule.Scripts
{
    public class InputManager : IInitializable ,IDisposable
    {
        public event Action OnLayoutChange;
        public InputSystem_Actions GameInput { get; private set; }
        public Layout CurrentLayout { get;private set; }
        
        public enum Layout
        {
            Gameplay,
            UI
        }
        
        public void Initialize()
        {
            GameInput = new InputSystem_Actions();
            GameInput.Gameplay.Enable();
        }
    
        public void SwitchToGameplay()
        {
            GameInput.UI.Disable();
            GameInput.Gameplay.Enable();
            
            CurrentLayout = Layout.Gameplay;
            OnLayoutChange?.Invoke();
        }
        
        public void SwitchToUI()
        {
            GameInput.Gameplay.Disable();
            GameInput.UI.Enable();
            
            CurrentLayout = Layout.UI;
            OnLayoutChange?.Invoke();
        }
        
        public void Dispose()
        {
            GameInput?.Dispose();
        }
    }
}