using UnityEngine;

namespace _Game.CodeBase.Features.BuildingModule.Scripts.RoomsAndObjects
{
    [ExecuteAlways]
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Collider2D))]
    public class WeightReceiver : MonoBehaviour
    {
        private const float SizeGridUnit = 0.5f;

        [SerializeField] private SpriteRenderer _spriteRenderer;

        [field: SerializeField] private BoxCollider2D _boxCollider;
        [field: SerializeField] public WeightData Data { get; private set; } = new();

        [SerializeField, HideInInspector] private Vector2 _cachedSize = Vector2.one;

        public BoxCollider2D BoxCollider => _boxCollider;

        public Transform Transform => transform;

        private void Awake()
        {
            Data.Position = transform.position;
        }

        private void UpdateSize()
        {
            var spriteSize = _spriteRenderer.sprite.rect.size / _spriteRenderer.sprite.pixelsPerUnit;

            _boxCollider.size = spriteSize;

            var rawSize = new Vector2(
                spriteSize.x * transform.localScale.x,
                spriteSize.y * transform.localScale.y
            );

            Data.Size = new Vector2(
                Mathf.Round(rawSize.x / SizeGridUnit) * SizeGridUnit,
                Mathf.Round(rawSize.y / SizeGridUnit) * SizeGridUnit
            );

            _cachedSize = Data.Size;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();

            if (_boxCollider == null)
                _boxCollider = GetComponent<BoxCollider2D>();

            UpdateSize();
        }
#endif
    }
}