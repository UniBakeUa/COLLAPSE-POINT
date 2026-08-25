using System;
using System.Collections.Generic;
using _Game.CodeBase.Features.BuildingModule.Scripts.Data;
using _Game.CodeBase.Features.BuildingModule.Scripts.Rooms;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Game.CodeBase.Features.BuildingModule.Scripts.Supports
{
    public class SupportsGenerator : IDisposable
    {
        private readonly SupportsConfig _config;
        private readonly ISupportFactory _factory;
        private readonly LayerMask _roomsLayerMask;

        private readonly Dictionary<int, int> _roomSupportCounters = new();
        private readonly Dictionary<int, SupportData> _activeSupports = new();
        private readonly Dictionary<int, Support> _activeInstances = new();

        private readonly RoomSpawner _roomSpawner;
        
        private static readonly RaycastHit2D[] _hitsBuffer = new RaycastHit2D[16];
        
        public SupportsGenerator(SupportsConfig config, ISupportFactory factory, LayerMask roomsLayerMask, RoomSpawner roomSpawner)
        {
            _config = config;
            _factory = factory;
            _roomsLayerMask = roomsLayerMask;
            _roomSpawner = roomSpawner;
        }

        public void SpawnRandomSupport(Room room, bool? horizontal = null)
        {
            bool isHorizontal = horizontal ?? Random.value > 0.5f;
            PlaceSupport(room, isHorizontal);
        }

        public void PlaceSupport(Room room, bool horizontal)
        {
            int startPointFailStreak = 0;

            for (int attempt = 0; attempt < 500; attempt++)
            {
                if (room == null)
                    break;
                
                Transform parentRoot = room.transform;
                int generation = 0;
                bool isFromSupport;
                float parentAngle = 0f;

                // Зменшили шанс рости з іншої підпірки, щоб не створювати густі "кущі" в одній точці
                bool wantFromSupport = room.Data.AttachedSupportIds.Count > 0 && Random.value < 0.2f;

                Vector2 start = wantFromSupport
                    ? TryGetPointFromExistingSupport(room, out parentRoot, out parentAngle, out generation)
                    : Vector2.zero;

                isFromSupport = start != Vector2.zero;

                if (!isFromSupport)
                {
                    start = GetRandomPointOnRoom(room);
                    parentRoot = room.transform;
                    generation = 0;
                }

                if (start == Vector2.zero)
                {
                    startPointFailStreak++;
                    if (startPointFailStreak > 20) return;
                    continue;
                }

                startPointFailStreak = 0;

                float newAngle = horizontal
                    ? (Random.value > 0.5f
                        ? -90 - Random.Range(_config.minHorizontalAngle, _config.maxHorizontalAngle)
                        : -90 + Random.Range(_config.minHorizontalAngle, _config.maxHorizontalAngle))
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

                float lengthBiasT = Mathf.Clamp01(room.Data.AttachedSupportIds.Count / (float)_config.longSupportRoomCountThreshold);
                float preferredMaxLength = Mathf.Lerp(_config.maxLength, _config.minLength, lengthBiasT);
                float currentMaxDist = horizontal ? (preferredMaxLength / 3f) : preferredMaxLength;
                
                Vector2 actualEnd = PerformPhysicsRaycast(start, dir, currentMaxDist, parentRoot);

                if (actualEnd == Vector2.zero) continue;

                float currentLength = Vector2.Distance(start, actualEnd);
                if (currentLength < _config.minLength ||
                    !ValidateConstraints(start, actualEnd, currentMaxDist, isFromSupport, horizontal))
                    continue;
                
                float lengthProgress = Mathf.InverseLerp(_config.minLength, _config.maxLength, currentLength);
                float currentThickness = Mathf.Lerp(_config.minThickness, _config.maxThickness, lengthProgress);

                int localId = GetNextLocalId(room.Data.Id);
                int compositeId = (room.Data.Id * 1000) + localId;
                
                var finalData = new SupportData(compositeId, start, actualEnd, room.Data.Id, generation, currentThickness);
                var material = _config.GetLevel(0);
                var instance = _factory.Create(finalData, material);

                _activeSupports[compositeId] = finalData;
                _activeInstances[compositeId] = instance;
                room.Data.AttachedSupportIds.Add(compositeId);
                return;
            }
        }

        private Vector2 PerformPhysicsRaycast(Vector2 start, Vector2 dir, float currentMaxDist, Transform ignoreRoot)
        {
            float verticality = Mathf.Abs(dir.y);
            float dynamicMaxDist = Mathf.Lerp(_config.minLength, currentMaxDist, verticality);

            Vector2 rayOrigin = start + dir * 0.05f;
            int hitCount = Physics2D.CircleCastNonAlloc(rayOrigin, 0.02f, dir, _hitsBuffer, dynamicMaxDist, _config.maskToCollide);

            for (int i = 0; i < hitCount; i++)
            {
                var hit = _hitsBuffer[i];
                if (hit.collider == null) continue;
                if (hit.collider.isTrigger) continue;
                if (ignoreRoot != null && hit.collider.transform.IsChildOf(ignoreRoot)) continue;
                if (hit.distance < 0.01f) continue;

                return hit.point;
            }

            var firstRoom = _roomSpawner.FirstRoom;
            if (firstRoom == null) return Vector2.zero;

            float groundY = firstRoom.transform.position.y - firstRoom.Data.Size.y / 2f - _config.maxLength;
            if (dir.y < 0)
            {
                float t = (groundY - start.y) / dir.y;
                if (t > 0 && t < dynamicMaxDist)
                    return start + dir * t;
            }

            return Vector2.zero;
        }

        private Vector2 TryGetPointFromExistingSupport(Room room, out Transform parentRoot, out float parentAngle, out int nextGeneration)
        {
            parentRoot = room.transform;
            parentAngle = 0f;
            nextGeneration = 0;

            if (room.Data.AttachedSupportIds.Count == 0) return Vector2.zero;

            int randomIndex = Random.Range(0, room.Data.AttachedSupportIds.Count);
            int parentId = room.Data.AttachedSupportIds[randomIndex];

            if (!_activeSupports.TryGetValue(parentId, out var parentData)) return Vector2.zero;

            float spawnChance = Mathf.Pow(0.4f, parentData.Generation); // Швидше затухання гілкування
            if (Random.value > spawnChance) return Vector2.zero;

            nextGeneration = parentData.Generation + 1;
            Vector2 parentDir = parentData.End - parentData.Start;
            parentAngle = Mathf.Atan2(parentDir.y, parentDir.x) * Mathf.Rad2Deg;

            if (_activeInstances.TryGetValue(parentId, out var parentInstance))
                parentRoot = parentInstance.transform;

            // Вибираємо випадкову точку на підпірці, але трохи відступивши від країв
            return parentData.GetPointOnLine(Random.Range(0.3f, 0.8f));
        }

        private bool ValidateConstraints(Vector2 start, Vector2 end, float currentMaxDist, bool isFromSupport, bool isHorizontalMode)
        {
            float length = Vector2.Distance(start, end);
            float dx = Mathf.Abs(start.x - end.x);
            float dy = Mathf.Abs(start.y - end.y);

            if (dy < 0.01f) return isHorizontalMode; 

            if (isHorizontalMode)
                return length <= currentMaxDist;

            float currentDrift = dx / dy;
            float strictDriftLimit = Mathf.Tan(5f * Mathf.Deg2Rad);
            float looseDriftLimit = _config.maxVerticalDriftRatio;

            float lengthFactor = Mathf.InverseLerp(_config.minLength, currentMaxDist, length);
            float dynamicMaxDrift = Mathf.Lerp(looseDriftLimit, strictDriftLimit, lengthFactor);

            if (!isFromSupport) dynamicMaxDrift /= 10f;

            return currentDrift <= dynamicMaxDrift;
        }

        private Vector2 GetRandomPointOnRoom(Room room)
        {
            var pos = room.transform.position;
            var size = room.Data.Size;

            float minX = pos.x - size.x / 2f;
            float maxX = pos.x + size.x / 2f;
            float y = pos.y - size.y / 2f;

            float safeMin = minX + 0.4f; // Збільшили відступи від країв кімнати
            float safeMax = maxX - 0.4f;

            var ownCollider = room.GetComponent<Collider2D>();

            for (int i = 0; i < 30; i++)
            {
                float testX = Random.Range(safeMin, safeMax);
                Vector2 checkPoint = new Vector2(testX, y - 0.05f);

                var hit = Physics2D.OverlapPoint(checkPoint, _roomsLayerMask);

                if (hit == null || hit == ownCollider)
                {
                    bool tooClose = false;
                    foreach (var id in room.Data.AttachedSupportIds)
                    {
                        if (_activeSupports.TryGetValue(id, out var data))
                        {
                            // Перевіряємо відстань не тільки по X, а по сукупній дистанції до інших точок стартів на цій кімнаті,
                            // щоб дошки не ліпишлись в один міліметр одна до одної
                            if (Mathf.Abs(data.Start.x - testX) < Mathf.Max(0.5f, _config.supportEdgeMargin))
                            {
                                tooClose = true;
                                break;
                            }
                        }
                    }

                    if (!tooClose) return new Vector2(testX, y);
                }
            }

            return Vector2.zero;
        }

        public void RemoveSupport(int id)
        {
            _activeSupports.Remove(id);

            if (_activeInstances.TryGetValue(id, out var instance))
            {
                instance.Dispose();
                _activeInstances.Remove(id);
            }
        }

        private int GetNextLocalId(int roomId)
        {
            if (!_roomSupportCounters.ContainsKey(roomId)) _roomSupportCounters[roomId] = 1;
            return _roomSupportCounters[roomId]++;
        }

        public void ClearAll()
        {
            foreach (var instance in _activeInstances.Values)
                instance.Dispose();

            _activeSupports.Clear();
            _activeInstances.Clear();
            _roomSupportCounters.Clear();
        }

        public void Dispose() => ClearAll();
    }
}