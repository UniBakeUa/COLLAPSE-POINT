using _Game.CodeBase.Features.BuildingModule.Scripts.Data;
using UnityEngine;

namespace _Game.CodeBase.Features.BuildingModule.Scripts.Rooms
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Room : MonoBehaviour, IAttachmentRules
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField, HideInInspector] private Vector2 _cachedSize = Vector2.one;

        public RoomData Data { get; private set; }
        public float Weight => Data.Weight;
        public Vector2 Size => _cachedSize;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();

            if (_spriteRenderer != null)
                _cachedSize = _spriteRenderer.bounds.size;
        }
#endif

        private void Reset()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Initialize(int id, Vector3 position, float weight)
        {
            Data = new RoomData
            {
                Id = id,
                Size = _cachedSize,
                Position = position,
                Weight = weight
            };

            transform.position = position;
        }
        

        // Дефолт: звичайна кімната дозволяє прикріплення з будь-якої сторони
        public virtual bool CanAttachOnSide(RoomSide side) => true;
    }
}