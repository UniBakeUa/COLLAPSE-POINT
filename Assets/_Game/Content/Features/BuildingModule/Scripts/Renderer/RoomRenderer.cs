using System;
using _Game.Content.Features.BuildingModule.Scripts.RoomSetup;
using UnityEngine;
using Zenject;

namespace _Game.Content.Features.BuildingModule.Scripts.Renderer
{
    public class RoomRenderer : MonoBehaviour
    {
        [SerializeField] private GameObject roomPrefab;

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

            GameObject roomGO = Instantiate(roomPrefab, transform);
            roomGO.name = $"Room_{room.Id}_{room.Difficulty}";

            var sr = roomGO.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = room.Form.GetSpriteForDifficulty(room.Difficulty);
            }

            roomGO.transform.localScale = room.Form.GetScaleForDifficulty(room.Difficulty);

            float posX = room.GridPosition.x + (room.Size.x - 1) / 2f;
            float posY = room.GridPosition.y + (room.Size.y - 1) / 2f;
            roomGO.transform.localPosition = new Vector3(posX, posY, 0);
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