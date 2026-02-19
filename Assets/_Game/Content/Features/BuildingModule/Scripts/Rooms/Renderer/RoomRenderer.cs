using System;
using System.Collections.Generic;
using _Game.Content.Features.BuildingModule.Scripts.RoomSetup;
using UnityEngine;
using Zenject;

namespace _Game.Content.Features.BuildingModule.Scripts.Renderer
{
    public class RoomRenderer : MonoBehaviour
    {
        [SerializeField] private Room roomPrefab;

        private RoomGenerator _roomGenerator;
        private RoomsSpawner _roomsSpawner;

        [Header("Editor Visual")] [SerializeField]
        private bool showGrid;
        
        [Inject]
        public void Construct(RoomGenerator roomGenerator, RoomsSpawner roomsSpawner)
        {
            _roomGenerator = roomGenerator;
            _roomsSpawner = roomsSpawner;
        }

        private void Awake()
        {
            _roomsSpawner.OnRoomGenerated += SpawnRoomVisual;
        }
        private void SpawnRoomVisual(RoomData room)
        {
            if (roomPrefab == null) return;

            Room roomGO = Instantiate(roomPrefab, transform);
            roomGO.name = $"Room_{room.Id}_{room.Difficulty}";
            
            room.RoomTransform = roomGO.transform;

            var sr = roomGO.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = room.Form.GetSpriteForDifficulty(room.Difficulty);
            }

            roomGO.transform.localScale = room.Form.GetScaleForDifficulty(room.Difficulty);

            float posX = room.GridPosition.x + room.Size.x / 2f;
            float posY = room.GridPosition.y + room.Size.y / 2f;
            roomGO.transform.localPosition = new Vector3(posX, posY, 0);
            
            UpdatePolygonCollider(roomGO.GetComponent<PolygonCollider2D>(), sr.sprite);
        }

        private void UpdatePolygonCollider(PolygonCollider2D collider, Sprite sprite)
        {
            if (sprite == null || collider == null) return;

            int shapeCount = sprite.GetPhysicsShapeCount();
            collider.pathCount = shapeCount;
            
            List<Vector2> pathPoints = new List<Vector2>();
    
            for (int i = 0; i < shapeCount; i++)
            {
                pathPoints.Clear();
                sprite.GetPhysicsShape(i, pathPoints);
                
                collider.SetPath(i, pathPoints); 
            }
        }
        private void OnDrawGizmos()
        {
            if (showGrid)
            {
                if (_roomGenerator == null) return;
                Gizmos.color = Color.cyan;
                foreach (var room in _roomGenerator.Rooms)
                {
                    Vector3 center = new Vector3(room.GridPosition.x + room.Size.x / 2f - 0.5f,
                        room.GridPosition.y + room.Size.y / 2f - 0.5f, 0);
                    Gizmos.DrawWireCube(center, new Vector3(room.Size.x, room.Size.y, 0.1f));
                }
            }
        }
    }
}