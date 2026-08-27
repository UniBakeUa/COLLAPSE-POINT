using UnityEngine;

namespace _Game.CodeBase.Features.Behaivors
{
    public class CameraRoomFramer : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private float _padding = 1f;

        [Header("Smoothing")]
        [SerializeField] private float _positionSmoothSpeed = 8f;
        [SerializeField] private float _zoomSmoothSpeed = 8f;

        private Vector3 _targetPosition;
        private float _targetOrthoSize;
        private bool _hasTarget;

        private void Awake()
        {
            if (_camera == null) _camera = Camera.main;

            if (_camera != null)
            {
                _targetPosition = transform.position;
                _targetOrthoSize = _camera.orthographicSize;
            }
        }

        private void LateUpdate()
        {
            if (!_hasTarget || _camera == null) return;

            transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * _positionSmoothSpeed);
            _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, _targetOrthoSize, Time.deltaTime * _zoomSmoothSpeed);
        }

        /// <summary>
        /// Задає нову ціль для камери — bounds кімнати з відступом _padding.
        /// Плавний перехід відбувається сам у LateUpdate.
        /// </summary>
        public void FrameBounds(Bounds bounds)
        {
            if (_camera == null) return;

            float boundsWidth = bounds.size.x + _padding * 2f;
            float boundsHeight = bounds.size.y + _padding * 2f;

            float screenAspect = (float)Screen.width / Screen.height;
            float boundsAspect = boundsWidth / boundsHeight;

            float requiredOrthoSize = boundsAspect > screenAspect
                ? boundsWidth / (2f * screenAspect)
                : boundsHeight / 2f;

            _targetPosition = new Vector3(bounds.center.x, bounds.center.y, transform.position.z);
            _targetOrthoSize = requiredOrthoSize;
            _hasTarget = true;
        }

        public void FrameRoom(Collider2D roomCollider)
        {
            if (roomCollider == null) return;
            FrameBounds(roomCollider.bounds);
        }

        /// <summary>
        /// Миттєво "снапає" камеру на ціль без плавного переходу (напр. при завантаженні сцени).
        /// </summary>
        public void SnapToBounds(Bounds bounds)
        {
            FrameBounds(bounds);
            transform.position = _targetPosition;
            if (_camera != null) _camera.orthographicSize = _targetOrthoSize;
        }
    }
}