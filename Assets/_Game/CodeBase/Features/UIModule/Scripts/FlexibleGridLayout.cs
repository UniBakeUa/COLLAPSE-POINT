using UnityEngine;
using UnityEngine.UI;

namespace _Game.CodeBase.Features.UIModule.Scripts
{
    public class FlexibleGridLayout : LayoutGroup
    {
        public enum FitType
        {
            Uniform,
            Width,
            Height,
            FixedRows,
            FixedColumns
        }

        public FitType fitType;
        public int rows;
        public int columns;
        public Vector2 cellSize;
        public Vector2 spacing;
        public bool fitX;
        public bool fitY;

        public override void SetLayoutHorizontal()
        {
            if (fitType == FitType.Width || fitType == FitType.Height || fitType == FitType.Uniform)
            {
                float sqrR = Mathf.Sqrt(transform.childCount);
                rows = Mathf.CeilToInt(sqrR);
                columns = Mathf.CeilToInt(sqrR);
            }

            if (fitType == FitType.Width || fitType == FitType.FixedColumns)
            {
                rows = Mathf.CeilToInt(transform.childCount / (float)columns);
            }
            else if (fitType == FitType.Height || fitType == FitType.FixedRows)
            {
                columns = Mathf.CeilToInt(transform.childCount / (float)rows);
            }

            float parentWidth = rectTransform.rect.width;
            float parentHeight = rectTransform.rect.height;

            float cellWidth = (parentWidth - (spacing.x * (columns - 1)) - (padding.left + padding.right)) /
                              (float)columns;
            float cellHeight = (parentHeight - (spacing.y * (rows - 1)) - (padding.top + padding.bottom)) / (float)rows;

            cellSize.x = fitX ? cellWidth : cellSize.x;
            cellSize.y = fitY ? cellHeight : cellSize.y;

            int columnCount = 0;
            int rowCount = 0;

            for (int i = 0; i < rectChildren.Count; i++)
            {
                rowCount = i / columns;
                columnCount = i % columns;

                var item = rectChildren[i];

                var xPos = (cellSize.x * columnCount) + (spacing.x * columnCount) + padding.left;
                var yPos = (cellSize.y * rowCount) + (spacing.y * rowCount) + padding.top;

                SetChildAlongAxis(item, 0, xPos, cellSize.x);
                SetChildAlongAxis(item, 1, yPos, cellSize.y);
            }
        }

        public override void SetLayoutVertical()
        {
            // Не використовується, оскільки розрахунок йде повністю у SetLayoutHorizontal
        }

        public override void CalculateLayoutInputVertical()
        {
            // Залишаємо порожнім
        }
    }
}