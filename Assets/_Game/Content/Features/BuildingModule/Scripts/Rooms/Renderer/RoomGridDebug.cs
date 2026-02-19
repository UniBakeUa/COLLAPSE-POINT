using UnityEditor;
using UnityEngine;
using Zenject;

namespace _Game.Content.Features.BuildingModule.Scripts.Renderer
{
    public class RoomGridDebug : MonoBehaviour
    {
        [Inject] private RoomGrid _grid;

        [Header("Transform Settings")] [SerializeField]
        private float _cellSize = 1f; // Розмір клітинки в метрах

        [SerializeField]
        private Vector3 _offset = new Vector3(0, 0.05f, 0); // Щоб сітка не "мерехтіла" всередині підлоги

        [Header("Visual Settings")] [SerializeField]
        private bool _drawLabels = true;

        [SerializeField] private Color _roomColor = new Color(0, 0.5f, 1f, 0.4f);
        [SerializeField] private Color _supportColor = new Color(0, 1f, 0, 0.4f);

        private void OnDrawGizmos()
        {
            if (_grid == null) return;

            // Матриця дозволяє малювати відносно позиції об'єкта Debug
            Gizmos.matrix = transform.localToWorldMatrix;

            // Debug.Log("______________");
            // foreach (var pair in _grid.GetAllCells())
            // {
            //     Debug.Log(pair.Value.OwnerId);
            // }
            // Debug.Log("______________");
            foreach (var pair in _grid.GetAllCells())
            {
                Vector2Int gridPos = pair.Key;
                RoomGrid.CellInfo info = pair.Value;

                // ПРАВИЛЬНО: x та y для 2D, z використовуємо лише для офсету (шарування)
                Vector3 localPos = new Vector3(gridPos.x * _cellSize, gridPos.y * _cellSize, 0) + _offset;

                // Обираємо колір
                Gizmos.color = info.Type == RoomGrid.CellType.Room ? _roomColor : _supportColor;

                // Малюємо плоский квадрат (розмір по Z мінімальний)
                Vector3 size = new Vector3(_cellSize * 0.9f, _cellSize * 0.9f, 0.01f);
                Gizmos.DrawCube(localPos, size);

                // Контур
                Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 1f);
                Gizmos.DrawWireCube(localPos, new Vector3(_cellSize, _cellSize, 0.01f));

#if UNITY_EDITOR
                if (_drawLabels)
                {
                    Vector3 worldPos = transform.TransformPoint(localPos);
                    // Виводимо тип клітинки для перевірки
                    string label = $"T:{info.Type}\nID:{info.OwnerId}";

                    GUIStyle style = new GUIStyle { fontSize = 10 };
                    style.normal.textColor = Color.white;

                    Handles.Label(worldPos, label, style);
                }
#endif
            }
        }
    }
}