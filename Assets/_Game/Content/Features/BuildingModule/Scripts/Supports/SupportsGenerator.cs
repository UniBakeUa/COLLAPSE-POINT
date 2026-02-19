using System;
using System.Collections.Generic;
using _Game.Content.Features.BuildingModule.Scripts.Renderer;
using UnityEngine;
using _Game.Content.Features.BuildingModule.Scripts.RoomSetup;
using _Game.Content.Features.BuildingModule.Scripts.Supports;
using _Game.Content.Features.Interfaces;
using Random = UnityEngine.Random;

namespace _Game.Content.Features.BuildingModule.Scripts
{
    public class SupportsGenerator : IDisposable
    {
        private readonly RoomGrid _grid;
        private readonly SupportsConfig _config;
        private readonly ISupportFactory _factory;

        // Tracks local ID count per room to generate composite IDs (e.g., Room 5 -> 5001)
        private readonly Dictionary<int, int> _roomSupportCounters = new();

        private readonly Dictionary<int, SupportData> _activeSupports = new();

        private readonly Dictionary<int, IRenderer<SupportData, SupportsConfig, SupportMaterialLevel>>
            _activeRenderers = new();

        public SupportsGenerator(RoomGrid grid, SupportsConfig config, ISupportFactory factory)
        {
            _grid = grid;
            _config = config;
            _factory = factory;
        }

        public void PlaceSupport(RoomData fromRoom, bool horizontal)
        {
            // --- 1. ШВИДКИЙ ПРЕ-ЧЕК (Запобіжник) ---
            bool roomHasSpace = false;
            for (int i = 0; i < 5; i++)
            {
                if (GetRandomPointOnRoom(fromRoom) != Vector2.zero)
                {
                    roomHasSpace = true;
                    break;
                }
            }

            // if (!roomHasSpace && fromRoom.AttachedSupportIds.Count == 0)
            // {
            //     Debug.LogWarning($"<color=yellow>[PlaceSupport]</color> Room {fromRoom.Id} is full. Skipping.");
            //     return;
            // }

            int startPointFailStreak = 0;

            for (int attempt = 0; attempt < 500; attempt++)
            {
                Vector2 start = Vector2.zero;
                Transform currentParent = fromRoom.RoomTransform;
                int generation = 0;
                float parentAngle = 0f;
                bool isFromSupport = false;

                // --- 2. ВИБІР ТОЧКИ СТАРТУ ---
                bool wantFromSupport = fromRoom.AttachedSupportIds.Count > 0 && Random.value < 0.7f;

                if (wantFromSupport)
                {
                    start = TryGetPointFromExistingSupport(fromRoom, out currentParent, out parentAngle,
                        out generation);
                    isFromSupport = start != Vector2.zero;
                }

                if (start == Vector2.zero)
                {
                    start = GetRandomPointOnRoom(fromRoom);
                    currentParent = fromRoom.RoomTransform;
                    generation = 0;
                    isFromSupport = false;
                }

                if (start == Vector2.zero)
                {
                    startPointFailStreak++;
                    if (startPointFailStreak > 20) return;
                    continue;
                }

                startPointFailStreak = 0;

                // --- 3. РОБОТА З КУТОМ ---
                float newAngle = horizontal
                    ? (Random.value > 0.5f ? -90 - Random.Range(_config.minHorizontalAngle, _config.maxHorizontalAngle) : -90 + Random.Range(_config.minHorizontalAngle, _config.maxHorizontalAngle))
                    : -90 + Random.Range(_config.minVerticalAngle, _config.maxVerticalAngle);

                if (isFromSupport)
                {
                    float diff = Mathf.Abs(Mathf.DeltaAngle(newAngle, parentAngle));
                    if (diff < _config.minAngleDifference)
                    {
                        float sign = Mathf.DeltaAngle(parentAngle, newAngle) >= 0 ? 1 : -1;
                        newAngle = parentAngle + (sign * _config.minAngleDifference);
                    }
                }

                Vector2 dir = new Vector2(Mathf.Cos(newAngle * Mathf.Deg2Rad), Mathf.Sin(newAngle * Mathf.Deg2Rad));

                // --- 4. ФІЗИКА ТА ІГНОРУВАННЯ КОЛАЙДЕРІВ ---
                int localId = GetNextLocalId(fromRoom.Id);
                int compositeId = (fromRoom.Id * 1000) + localId;
                var material = _config.GetLevel(0);

                var renderer =
                    _factory.Create(new SupportData(compositeId, start, start + dir, fromRoom.Id, generation), _config,
                        material);

                if (renderer is SupportRenderer physRenderer)
                {
                    // ВИЗНАЧАЄМО, ЩО САМЕ ІГНОРУВАТИ (Твій запит)
                    Collider2D colliderToIgnore = null;
                    if (isFromSupport && currentParent != null)
                    {
                        // Якщо ростемо від підтримки, ігноруємо колайдер ЦІЄЇ підтримки
                        colliderToIgnore = currentParent.GetComponent<Collider2D>();
                    }
                    else
                    {
                        // Якщо від кімнати — ігноруємо колайдер кімнати
                        colliderToIgnore = fromRoom.RoomTransform.GetComponent<Collider2D>();
                    }
                    
                    float currentMaxDist = horizontal ? (_config.maxLength / 3f) : _config.maxLength;
                    
                    Vector2 actualEnd = physRenderer.PerformPhysicsRaycast(start, dir, currentMaxDist, _config.maskToCollide, colliderToIgnore);
                    
                    if (actualEnd == Vector2.zero) 
                    {
                        physRenderer.Dispose();
                        continue; 
                    }
                    
                    float currentLength = Vector2.Distance(start, actualEnd);

                    // --- 5. ВАЛІДАЦІЯ ---
                    if (currentLength >= _config.minLength && ValidateConstraints(start, actualEnd, currentMaxDist, isFromSupport, horizontal))
                    {
                        var finalData = new SupportData(compositeId, start, actualEnd, fromRoom.Id, generation);

                        _activeSupports[compositeId] = finalData;
                        _activeRenderers[compositeId] = physRenderer;

                        FinalizeSupport(finalData);
                        physRenderer.Initialize(finalData, _config, material, currentParent);
                        fromRoom.AttachedSupportIds.Add(compositeId);
                        return;
                    }
                    else
                    {
                        // Якщо не пройшло валідацію — видаляємо тимчасовий об'єкт
                        physRenderer.Dispose();
                    }
                }
            }
        }

        private Vector2 TryGetPointFromExistingSupport(RoomData fromRoom, out Transform parentTransform,
            out float parentAngle, out int nextGeneration)
        {
            parentTransform = fromRoom.RoomTransform;
            parentAngle = 0f;
            nextGeneration = 0;

            if (fromRoom.AttachedSupportIds.Count == 0) return Vector2.zero;

            // 1. Вибираємо випадкову існуючу підпорку
            int randomIndex = Random.Range(0, fromRoom.AttachedSupportIds.Count);
            int parentId = fromRoom.AttachedSupportIds[randomIndex];

            if (_activeSupports.TryGetValue(parentId, out var parentData))
            {
                // 2. Розраховуємо шанс спавну на основі покоління
                // Формула: 1.0 (100%) для 0 покоління, 0.5 (50%) для 1-го, 0.25 для 2-го і т.д.
                // Або просто лінійно: float spawnChance = Mathf.Max(0.1f, 1.0f - (parentData.Generation * 0.3f));
                float spawnChance = Mathf.Pow(0.5f, parentData.Generation);

                if (Random.value > spawnChance) return Vector2.zero;

                // 3. Якщо пройшли перевірку рандомом — готуємо дані дитини
                nextGeneration = parentData.Generation + 1;
                Vector2 parentDir = parentData.End - parentData.Start;
                parentAngle = Mathf.Atan2(parentDir.y, parentDir.x) * Mathf.Rad2Deg;

                if (_activeRenderers.TryGetValue(parentId, out var parentRenderer))
                {
                    parentTransform = ((Component)parentRenderer).transform;
                }

                return
                    parentData.GetPointOnLine(Random.Range(0.5f,
                        0.95f)); //ЩОБ НЕ СПАВНИТИ НА САМОМУ КІНЦІ ЧИ ПОЧАТКУ БАЛКИ
            }

            return Vector2.zero;
        }

        private void FinalizeSupport(SupportData data)
        {
            _activeSupports[data.Id] = data;
            MarkSupportInGrid(data.Start, data.End, data.Id);

            bool isVertical = Mathf.Abs(data.Start.x - data.End.x) < Mathf.Abs(data.Start.y - data.End.y);
            foreach (var cell in GetLineCells(data.Start, data.End))
            {
                _grid.BlockCell(cell, isVertical, data.Id);
            }
        }

        private void MarkSupportInGrid(Vector2 start, Vector2 end, int supportId)
        {
            float distance = Vector2.Distance(start, end);
            Vector2 direction = (end - start).normalized;
            float step = 0.5f;

            for (float d = 0; d <= distance; d += step)
            {
                Vector2 point = start + direction * d;
                Vector2Int gridPos = new Vector2Int(Mathf.FloorToInt(point.x), Mathf.FloorToInt(point.y));
                _grid.OccupyAsSupport(gridPos, supportId);
            }
        }

        public void RemoveSupport(int id)
        {
            _grid.RemoveAllCellsByOwnerId(id);
            _activeSupports.Remove(id);

            if (_activeRenderers.TryGetValue(id, out var renderer))
            {
                renderer.Dispose();
                _activeRenderers.Remove(id);
            }
        }

        private bool ValidateConstraints(Vector2 start, Vector2 end, float currentMaxDist, bool isFromSupport, bool isHorizontalMode)
        {
            float length = Vector2.Distance(start, end);
            float dx = Mathf.Abs(start.x - end.x);
            float dy = Mathf.Abs(start.y - end.y);

            if (dy < 0.01f) return isHorizontalMode; // Дозволяємо горизонтальним бути майже паралельними підлозі
            
            if (isHorizontalMode)
            {
                return length <= currentMaxDist; 
            }

            // 2. Логіка для ВЕРТИКАЛЬНИХ балок (те, що ти просив про 5 градусів)
            float currentDrift = dx / dy;
            float strictDriftLimit = Mathf.Tan(5f * Mathf.Deg2Rad); 
            float looseDriftLimit = _config.maxVerticalDriftRatio;

            float lengthFactor = Mathf.InverseLerp(_config.minLength, currentMaxDist, length);
            float dynamicMaxDrift = Mathf.Lerp(looseDriftLimit, strictDriftLimit, lengthFactor);

            if (!isFromSupport) dynamicMaxDrift /= 10f;

            return currentDrift <= dynamicMaxDrift;
        }

        private Vector2 GetRandomPointOnRoom(RoomData room)
        {
            float minX = room.GridPosition.x;
            float maxX = room.GridPosition.x + room.Size.x;
            float y = room.GridPosition.y;

            // Робимо невеликий відступ, щоб балки не спавнилися на кутах
            float safeMin = minX + 0.2f;
            float safeMax = maxX - 0.2f;

            for (int i = 0; i < 30; i++)
            {
                float testX = Random.Range(safeMin, safeMax);

                // Перевіряємо клітинку під підлогою
                Vector2Int checkPos = new Vector2Int(Mathf.FloorToInt(testX), Mathf.FloorToInt(y) - 1);
                var cell = _grid.GetCell(checkPos);

                // КЛЮЧОВА ПРАВКА:
                // Ми ігноруємо те, що клітинка може бути зайнята типом Support (зелена).
                // Ми зупиняємося ТІЛЬКИ якщо там Room (синя).
                if (cell == null || cell.Value.Type != RoomGrid.CellType.Room)
                {
                    // Перевірка на "злипання" з уже існуючими балками ЦІЄЇ кімнати
                    bool tooClose = false;
                    foreach (var id in room.AttachedSupportIds)
                    {
                        if (_activeSupports.TryGetValue(id, out var data) &&
                            Mathf.Abs(data.Start.x - testX) < _config.supportEdgeMargin)
                        {
                            tooClose = true;
                            break;
                        }
                    }

                    if (!tooClose) return new Vector2(testX, y);
                }
            }

            return Vector2.zero;
        }

        private List<Vector2Int> GetLineCells(Vector2 start, Vector2 end)
        {
            List<Vector2Int> cells = new();
            float dist = Vector2.Distance(start, end);

            for (float i = 0; i <= dist; i += 0.5f)
            {
                Vector2 p = Vector2.Lerp(start, end, i / dist);
                Vector2Int cell = new Vector2Int(Mathf.FloorToInt(p.x), Mathf.FloorToInt(p.y));
                if (!cells.Contains(cell)) cells.Add(cell);
            }

            return cells;
        }

        private int GetNextLocalId(int roomId)
        {
            if (!_roomSupportCounters.ContainsKey(roomId)) _roomSupportCounters[roomId] = 1;
            return _roomSupportCounters[roomId]++;
        }

        public void DestroyAll()
        {
            foreach (var renderer in _activeRenderers.Values)
            {
                renderer.Dispose();
            }
        }

        public void ClearAll()
        {
            _activeSupports.Clear();
            _activeRenderers.Clear();
            _roomSupportCounters.Clear();

            Debug.Log("Supports fully cleared.");
        }

        public void Dispose()
        {
            ClearAll();
        }
    }
}