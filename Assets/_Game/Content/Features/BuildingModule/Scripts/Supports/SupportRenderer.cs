using _Game.Content.Features.BuildingModule.Scripts.Supports;
using _Game.Content.Features.Interfaces;
using UnityEngine;
using Zenject;

namespace _Game.Content.Features.BuildingModule.Scripts.Renderer
{
    public class SupportRenderer : MonoBehaviour, IRenderer<SupportData, SupportsConfig, SupportMaterialLevel>
    {
        private SpriteRenderer _spriteRenderer;

        private BoxCollider2D _boxCollider;

        [Inject] private SupportsConfig _config;

        public Vector2 PerformPhysicsRaycast(Vector2 start, Vector2 dir, float unused, LayerMask mask, Collider2D ignoreCollider = null)
        {
            // 1. Обчислюємо вертикальність (від 0 до 1)
            // Mathf.Abs(dir.y) дає 1.0, якщо балка ідеально вертикальна, і 0.0, якщо горизонтальна
            float verticality = Mathf.Abs(dir.y);

            // 2. Розраховуємо динамічний ліміт:
            // Чим менша вертикальність, тим ближче результат до minLength.
            // Чим більша вертикальність, тим ближче до maxLength.
            float dynamicMaxDist = Mathf.Lerp(_config.minLength, _config.maxLength, verticality);

            Vector2 rayOrigin = start + dir * 0.01f; 
    
            // Використовуємо динамічну дистанцію для CircleCast
            RaycastHit2D[] hits = Physics2D.CircleCastAll(rayOrigin, 0.02f, dir, dynamicMaxDist, mask);

            foreach (var hit in hits)
            {
                if (hit.collider == null) continue;
                if (ignoreCollider != null && hit.collider == ignoreCollider) continue;
                if (hit.collider.isTrigger) continue;
                if (hit.distance < 0.01f) continue;

                return hit.point;
            }

            // ГЛОБАЛЬНА ПЕРЕВІРКА ЗЕМЛІ
            float groundY = -10f; 
            if (dir.y < 0)
            {
                float t = (groundY - start.y) / dir.y;
                // Перевіряємо, чи входить точка землі в наш динамічний ліміт
                if (t > 0 && t < dynamicMaxDist) 
                {
                    return start + dir * t;
                }
            }

            // Якщо ні в що не влучили в межах нашої "обрізаної" дистанції
            return Vector2.zero; 
        }

        public void Initialize(SupportData data, SupportsConfig config, SupportMaterialLevel material, Transform parent = null)
        {
            if (parent != null)
            {
                transform.SetParent(parent, true);
            }

            Vector2 dir = data.End - data.Start;
            float length = dir.magnitude;
            
            transform.position = data.Start; 
            
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

            _spriteRenderer = GetComponent<SpriteRenderer>();
            _boxCollider = GetComponent<BoxCollider2D>();
            
            _spriteRenderer.size = new Vector2(length, config.thickness);
            _spriteRenderer.color = material.visualColor;
            
            _boxCollider.size = new Vector2(length, config.thickness);
            _boxCollider.offset = new Vector2(length / 2f, 0);

            Physics2D.SyncTransforms();
        }

        public void Dispose() => Destroy(gameObject);
    }
}