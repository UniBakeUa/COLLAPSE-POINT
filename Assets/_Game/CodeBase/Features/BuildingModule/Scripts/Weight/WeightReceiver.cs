using _Game.CodeBase.Features.BuildingModule.Scripts.RoomsAndObjects;
using UnityEngine;

namespace _Game.CodeBase.Features.BuildingModule.Scripts.Weight
{
    [ExecuteAlways]
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Collider2D))]
    public class WeightReceiver : MonoBehaviour
    {
        private const float SizeGridUnit = 0.5f;

        [field: SerializeField] private BoxCollider2D _boxCollider;
        [field: SerializeField] public WeightData Data { get; private set; } = new();

        [SerializeField] private SpriteRenderer _spriteRenderer;

        [SerializeField, HideInInspector] private Vector2 _cachedSize = Vector2.one;
        private InfiniteAnchorReceiver _infiniteAnchor;

        [Header("Stress Visualization")] // ДОДАНО
        [SerializeField] private Color _normalColor = Color.white; // ДОДАНО
        [SerializeField] private Color _overloadedColor = Color.red; // ДОДАНО
        [SerializeField] private float _stressVisualizationDivisor = 1f; // ДОДАНО: наскільки NotStabilizedLoad/TotalWeight має бути, щоб дійти до 100% червоного

        public BoxCollider2D BoxCollider => _boxCollider;
        public bool IsInfiniteAnchor => _infiniteAnchor != null;
        public Transform Transform => transform;

        private void Awake()
        {
            Data.Position = transform.position;
            TryGetComponent<InfiniteAnchorReceiver>(out var infiniteAnchor);
            _infiniteAnchor = infiniteAnchor;

            Data.OnWeightChanged += RefreshStressVisual; // ДОДАНО
        }

        private void OnDestroy() // ДОДАНО
        {
            Data.OnWeightChanged -= RefreshStressVisual;
        }

        // ДОДАНО: 0 = без навантаження, 1+ = повністю перевантажена (клемпиться для кольору)
        public float GetStressFactor()
        {
            if (IsInfiniteAnchor) return 0f;

            float total = Data.BaseWeight + Data.ReceivedLoad;
            if (total <= 0f) return 0f;

            float divisor = Mathf.Max(_stressVisualizationDivisor, 0.0001f);
            return Data.NotStabilizedLoad / (total * divisor);
        }

        // ДОДАНО: перефарбовує спрайт від normalColor до overloadedColor залежно від стресу
        private void RefreshStressVisual()
        {
            if (_spriteRenderer == null) return;

            float stress = Mathf.Clamp01(GetStressFactor());
            _spriteRenderer.color = Color.Lerp(_normalColor, _overloadedColor, stress);
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