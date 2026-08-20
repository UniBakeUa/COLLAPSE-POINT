using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace _Game.CodeBase.Features.UIModule.Scripts
{
   public class WindowManager : IWindowManager, IInitializable, IDisposable, ITickable
    {
        private readonly Dictionary<Type, UIWindowViewBase> _screens;
        private readonly Stack<UIWindowViewBase> _overlayStack = new();

        public UIWindowViewBase CurrentMain { get; private set; }
        public UIWindowViewBase TopOverlay => _overlayStack.Count > 0 ? _overlayStack.Peek() : null;

        public WindowManager(List<UIWindowViewBase> allScreens)
        {
            _screens = allScreens.ToDictionary(s => s.GetType());
        }

        public void Initialize() { }

        public void Dispose() { }

        public void Tick()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                TryCloseTop();
        }

        public T OpenMain<T>() where T : UIWindowViewBase
        {
            if (CurrentMain != null && CurrentMain.GetType() == typeof(T))
                return CurrentMain as T;

            CurrentMain?.Hide();

            var screen = GetScreen<T>();
            screen.Show();
            CurrentMain = screen;

            return screen;
        }

        public T OpenOverlay<T>() where T : UIWindowViewBase
        {
            var screen = GetScreen<T>();
            screen.Show();
            _overlayStack.Push(screen);

            return screen;
        }

        public void CloseTopOverlay()
        {
            if (_overlayStack.Count == 0) return;

            var top = _overlayStack.Pop();
            top.Hide();
        }

        public void CloseAllOverlays()
        {
            while (_overlayStack.Count > 0)
                CloseTopOverlay();
        }

        public bool TryCloseTop()
        {
            if (_overlayStack.Count == 0) return false;

            CloseTopOverlay();
            return true;
        }

        private T GetScreen<T>() where T : UIWindowViewBase
        {
            if (!_screens.TryGetValue(typeof(T), out var screen))
                throw new InvalidOperationException($"Screen of type {typeof(T).Name} is not registered");

            return screen as T;
        }
    }
}