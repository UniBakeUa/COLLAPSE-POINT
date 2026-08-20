using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Zenject;

namespace _Game.Core.InputSystemModule.Scripts
{
    public class InputService : IInputService, IInitializable, IDisposable
    {
        public event Action<Vector2> OnSimpleClick;
        public event Action<Vector2> OnDragStart;
        public event Action<Vector2> OnDragging;
        public event Action OnDragEnd;
        public event Action OnLayoutChange;

        public InputSystemm_Actions GameInput { get; private set; }
        public Layout CurrentLayout { get; private set; }

        private Vector2 _startMousePosition;
        private bool _isPotentialDrag;
        private bool _isDragging;
        private const float DragThreshold = 10f;

        public enum Layout
        {
            Gameplay,
            UI
        }

        public void Initialize()
        {
            GameInput = new InputSystemm_Actions();
            SubscribeEvents();

            SwitchToUI();
        }

        public void Dispose()
        {
            if (GameInput == null) return;

            GameInput.Gameplay.Click.started -= OnPressStarted;
            GameInput.Gameplay.Click.canceled -= OnPressCanceled;
            GameInput.Dispose();
        }

        #region Layout switching
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
        #endregion

        #region Pointer / drag detection
        private void SubscribeEvents()
        {
            GameInput.Gameplay.Click.started += OnPressStarted;
            GameInput.Gameplay.Click.canceled += OnPressCanceled;
        }

        private void OnPressStarted(InputAction.CallbackContext ctx)
        {
            _startMousePosition = GetPointerPosition();
            _isPotentialDrag = true;
            _isDragging = false;
        }

        private void OnPressCanceled(InputAction.CallbackContext ctx)
        {
            if (_isDragging)
            {
                OnDragEnd?.Invoke();
            }
            else if (_isPotentialDrag && !IsPointerOverUI())
            {
                OnSimpleClick?.Invoke(_startMousePosition);
            }

            ResetDragState();
        }

        private void CheckForDragStart(Vector2 currentPos)
        {
            if (Vector2.Distance(_startMousePosition, currentPos) <= DragThreshold) return;

            _isDragging = true;
            OnDragStart?.Invoke(_startMousePosition);
        }

        private void ResetDragState()
        {
            _isPotentialDrag = false;
            _isDragging = false;
        }

        private bool IsPointerOverUI()
        {
            if (EventSystem.current == null) return false;

            bool isTouchActive = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed;

            return isTouchActive
                ? EventSystem.current.IsPointerOverGameObject(Touchscreen.current.primaryTouch.touchId.ReadValue())
                : EventSystem.current.IsPointerOverGameObject();
        }

        public Vector2 GetPointerPosition()
        {
            return GameInput.Gameplay.Point.ReadValue<Vector2>();
        }
        #endregion
    }
}