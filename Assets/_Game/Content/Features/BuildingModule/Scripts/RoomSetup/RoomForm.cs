using System;
using UnityEngine;

namespace _Game.Content.Features.BuildingModule.Scripts.RoomSetup
{
    [Serializable]
    public class RoomForm
    {
        public string formName;
        public Sprite[] sprites; 

        public Vector2Int GetGridSize(RoomData.RoomDifficulty difficulty)
        {
            Sprite sprite = GetSpriteForDifficulty(difficulty);
            if (sprite == null) return Vector2Int.one;

            int width = Mathf.RoundToInt(sprite.bounds.size.x);
            int height = Mathf.RoundToInt(sprite.bounds.size.y);

            return new Vector2Int(Mathf.Max(1, width), Mathf.Max(1, height));
        }

        public Vector3 GetScaleForDifficulty(RoomData.RoomDifficulty difficulty)
        {
            Sprite sprite = GetSpriteForDifficulty(difficulty);
            if (sprite == null) return Vector3.one;

            Vector2Int gridSize = GetGridSize(difficulty);
            
            float scaleX = gridSize.x / sprite.bounds.size.x;
            float scaleY = gridSize.y / sprite.bounds.size.y;

            return new Vector3(scaleX, scaleY, 1);
        }

        public Sprite GetSpriteForDifficulty(RoomData.RoomDifficulty difficulty)
        {
            int index = (int)difficulty; // Zero=0, Normal=1 і т.д.
            if (sprites == null || index >= sprites.Length) 
            {
                Debug.LogWarning($"Sprite {difficulty} not found in {formName}");
                return sprites.Length > 0 ? sprites[0] : null;
            }
            return sprites[index];
        }
    }
}