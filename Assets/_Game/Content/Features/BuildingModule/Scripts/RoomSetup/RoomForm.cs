using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Game.Content.Features.BuildingModule.Scripts.RoomSetup
{
    [Serializable]
    public class RoomForm
    {
        public string formName;
        public Sprite[] sprites;
        [Range(0f, 1f)] public float alphaThreshold = 0.1f;

        [SerializeField, HideInInspector] private List<DifficultyMask> savedMasks = new();
        [SerializeField, HideInInspector] private Vector2Int cachedGridSize;

        [Serializable]
        private class DifficultyMask
        {
            public RoomData.RoomDifficulty difficulty;
            public List<Vector2Int> cells;
        }

        public List<Vector2Int> GetOccupiedCells(RoomData.RoomDifficulty difficulty)
        {
            var mask = savedMasks.Find(m => m.difficulty == difficulty);
            if (mask != null && mask.cells != null && mask.cells.Count > 0)
                return mask.cells;

            return new List<Vector2Int> { Vector2Int.zero };
        }

        public Sprite GetSpriteForDifficulty(RoomData.RoomDifficulty difficulty)
        {
            int index = (int)difficulty;
            if (sprites == null || sprites.Length == 0) return null;
            
            if (index >= sprites.Length || sprites[index] == null)
                return sprites[0];

            return sprites[index];
        }

        public Vector3 GetScaleForDifficulty(RoomData.RoomDifficulty difficulty)
        {
            Sprite sprite = GetSpriteForDifficulty(difficulty);
            if (sprite == null) return Vector3.one;

            float spriteW = sprite.bounds.size.x;
            float spriteH = sprite.bounds.size.y;

            float scaleX = cachedGridSize.x / spriteW;
            float scaleY = cachedGridSize.y / spriteH;

            return new Vector3(scaleX, scaleY, 1);
        }

        public Vector2Int GetGridSize() => cachedGridSize;

        public void BakeLayout()
        {
            savedMasks.Clear();
            if (sprites == null || sprites.Length == 0 || sprites[0] == null) return;

            cachedGridSize = new Vector2Int(
                Mathf.RoundToInt(sprites[0].bounds.size.x),
                Mathf.RoundToInt(sprites[0].bounds.size.y)
            );

            for (int i = 0; i < sprites.Length; i++)
            {
                if (sprites[i] == null) continue;
                
                var diff = (RoomData.RoomDifficulty)i;
                var occupied = ScanSprite(sprites[i]);
                
                savedMasks.Add(new DifficultyMask { difficulty = diff, cells = occupied });
            }
        }

        private List<Vector2Int> ScanSprite(Sprite sprite)
        {
            List<Vector2Int> occupied = new();
            Texture2D tex = sprite.texture;
            Rect rect = sprite.textureRect;
            float ppu = sprite.pixelsPerUnit;

            int gridW = Mathf.RoundToInt(sprite.bounds.size.x);
            int gridH = Mathf.RoundToInt(sprite.bounds.size.y);

            for (int x = 0; x < gridW; x++)
            {
                for (int y = 0; y < gridH; y++)
                {
                    int startX = (int)(rect.x + x * ppu);
                    int startY = (int)(rect.y + y * ppu);
                    int endX = (int)(startX + ppu);
                    int endY = (int)(startY + ppu);

                    bool cellIsOccupied = false;
                    
                    for (int px = startX; px < endX; px += 2)
                    {
                        for (int py = startY; py < endY; py += 2)
                        {
                            if (tex.GetPixel(px, py).a > alphaThreshold)
                            {
                                cellIsOccupied = true;
                                break;
                            }
                        }
                        if (cellIsOccupied) break;
                    }

                    if (cellIsOccupied)
                    {
                        occupied.Add(new Vector2Int(x, y));
                    }
                }
            }
            return occupied;
        }
    }
}