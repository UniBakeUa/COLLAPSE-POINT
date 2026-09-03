using _Game.Core.InputSystemModule.Scripts;
using UnityEngine;
using Zenject;

namespace _Game.CodeBase.Features.Behaivors
{
    public class CameraRoomFramer : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private float _padding = 1f;

        [Header("Smoothing")]
        [SerializeField] private float _positionSmoothSpeed = 8f;
        [SerializeField] private float _zoomSmoothSpeed = 8f;

        [Header("Manual Zoom & Pan")]
        [SerializeField] private float _minOrthoSize = 2f;
        [SerializeField] private float _zoomSpeed = 5f;
        [SerializeField] private float _panSpeed = 0.01f;

        [Inject] private IInputService _inputService;
        
        private Vector3 _targetPosition;
        private float _targetOrthoSize;
        private bool _hasTarget;

        private float _autoMaxOrthoSize; 
        private bool _manualZoomActive;  
        private bool _manualPanActive;   
        
        private Bounds _currentBounds; // Зберігаємо поточні межі для обмеження перетягування

        private void Awake()
        {
            if (_camera == null) _camera = Camera.main;

            if (_camera != null)
            {
                _targetPosition = transform.position;
                _targetOrthoSize = _camera.orthographicSize;
                _autoMaxOrthoSize = _camera.orthographicSize;
            }
        }

        private void OnEnable()
        {
            if (_inputService != null)
            {
                _inputService.OnScroll += HandleScroll;
                _inputService.OnDragStart += HandleDragStart;
                _inputService.OnDragging += HandleDragging;
            }
        }

        private void OnDisable()
        {
            if (_inputService != null)
            {
                _inputService.OnScroll -= HandleScroll;
                _inputService.OnDragStart -= HandleDragStart;
                _inputService.OnDragging -= HandleDragging;
            }
        }

        private void HandleScroll(float scrollValue)
        {
            float delta = Mathf.Sign(scrollValue) * 0.5f; 
            ApplyZoomDelta(delta);
        }

        private void HandleDragStart(Vector2 position)
        {
            _manualPanActive = true;
        }

        private void HandleDragging(Vector2 deltaPosition)
        {
            if (!_manualPanActive || _camera == null) return;

            float currentPanSpeed = _panSpeed * (_camera.orthographicSize / 5f);
            Vector3 move = new Vector3(-deltaPosition.x * currentPanSpeed, -deltaPosition.y * currentPanSpeed, 0f);
            
            _targetPosition += move;
            
            // Обмежуємо позицію в межах кімнати, щоб камера не виїжджала занадто далеко
            _targetPosition = ClampPositionToBounds(_targetPosition);
            
            _hasTarget = true;
        }

        private void LateUpdate()
        {
            if (!_hasTarget || _camera == null) return;

            transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * _positionSmoothSpeed);
            _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, _targetOrthoSize, Time.deltaTime * _zoomSmoothSpeed);
        }

        public void FrameBounds(Bounds bounds)
        {
            if (_camera == null) return;

            _currentBounds = bounds; // Зберігаємо межі кімнати

            float boundsWidth = bounds.size.x + _padding * 2f;
            float boundsHeight = bounds.size.y + _padding * 2f;

            float screenAspect = (float)Screen.width / Screen.height;
            float boundsAspect = boundsWidth / boundsHeight;

            float requiredOrthoSize = boundsAspect > screenAspect
                ? boundsWidth / (2f * screenAspect)
                : boundsHeight / 2f;

            _autoMaxOrthoSize = requiredOrthoSize; 

            if (!_manualPanActive)
            {
                _targetPosition = new Vector3(bounds.center.x, bounds.center.y, transform.position.z);
            }

            if (!_manualZoomActive) 
            {
                _targetOrthoSize = requiredOrthoSize;
            }
            else
            {
                _targetOrthoSize = Mathf.Clamp(_targetOrthoSize, _minOrthoSize, _autoMaxOrthoSize); 
            }

            // Навіть якщо не рухаємо мишею, перевіряємо, чи не виходить ціль за нові межі
            _targetPosition = ClampPositionToBounds(_targetPosition);

            _hasTarget = true;
        }

        private Vector3 ClampPositionToBounds(Vector3 targetPos)
        {
            if (_camera == null) return targetPos;

            // Обчислюємо видиму область камери (половину ширини та висоти у світі)
            float vertExtent = _targetOrthoSize;
            float horzExtent = vertExtent * ((float)Screen.width / Screen.height);

            // Визначаємо мінімальні та максимальні межі для центру камери з урахуванням габаритів кімнати та паддінгу
            float minX = _currentBounds.min.x - _padding + horzExtent;
            float maxX = _currentBounds.max.x + _padding - horzExtent;
            float minY = _currentBounds.min.y - _padding + vertExtent;
            float maxY = _currentBounds.max.y + _padding - vertExtent;

            // Якщо кімната менша за екран камери у цьому зумі, центруємо її
            if (minX > maxX)
            {
                targetPos.x = _currentBounds.center.x;
            }
            else
            {
                targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
            }

            if (minY > maxY)
            {
                targetPos.y = _currentBounds.center.y;
            }
            else
            {
                targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);
            }

            return targetPos;
        }

        public void FrameRoom(Collider2D roomCollider)
        {
            if (roomCollider == null) return;
            FrameBounds(roomCollider.bounds);
        }

        public void SnapToBounds(Bounds bounds)
        {
            _manualZoomActive = false; 
            _manualPanActive = false;  
            FrameBounds(bounds);
            transform.position = _targetPosition;
            if (_camera != null) _camera.orthographicSize = _targetOrthoSize;
        }

        public void ApplyZoomDelta(float delta)
        {
            _manualZoomActive = true;

            float newSize = _targetOrthoSize - delta * _zoomSpeed;
            _targetOrthoSize = Mathf.Clamp(newSize, _minOrthoSize, _autoMaxOrthoSize);

            // Оновлюємо обмеження позиції при зміні зуму (коли наближаємо/віддаляємося, розмір вікна камери змінюється)
            _targetPosition = ClampPositionToBounds(_targetPosition);

            if (Mathf.Abs(_targetOrthoSize - _autoMaxOrthoSize) < 0.001f && delta < 0f)
            {
                ResetToAutoFrame();
            }
        }

        public void ResetToAutoFrame()
        {
            _manualZoomActive = false;
            _manualPanActive = false; 
            _targetOrthoSize = _autoMaxOrthoSize;
            _targetPosition = new Vector3(_currentBounds.center.x, _currentBounds.center.y, transform.position.z);
        }

        public bool IsManualZoomActive => _manualZoomActive;
        public bool IsManualPanActive => _manualPanActive;
    }
}