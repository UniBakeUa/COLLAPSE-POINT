using System;
using System.Collections.Generic;
using _Game.CodeBase.Features.BuildingModule.Scripts.Data;
using _Game.CodeBase.Features.BuildingModule.Scripts.RoomsAndObjects;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Game.CodeBase.Features.BuildingModule.Scripts.Supports
{
    public class SupportsGenerator : IDisposable
    {
        private readonly SupportsConfig _config;
        private readonly ISupportFactory _factory;
        private readonly LayerMask _roomsLayerMask;

        private readonly Dictionary<int, int> _receiverSupportCounters = new();
        private readonly Dictionary<int, SupportData> _activeSupports = new();
        private readonly Dictionary<int, Support> _activeInstances = new();
        private readonly List<Vector2> _usedStartPoints = new();

        private readonly Dictionary<int, int> _maxSupportsForReceiver = new();
        private readonly Dictionary<int, int> _supportMaterialLevel = new();

        private readonly RoomSpawner _roomSpawner;

        private static readonly RaycastHit2D[] _hitsBuffer = new RaycastHit2D[16];

        public SupportsGenerator(SupportsConfig config, ISupportFactory factory, LayerMask roomsLayerMask,
            RoomSpawner roomSpawner)
        {
            _config = config;
            _factory = factory;
            _roomsLayerMask = roomsLayerMask;
            _roomSpawner = roomSpawner;
        }

        public void SpawnRandomSupport(WeightReceiver receiver, bool? horizontal = null)
        {
            bool isHorizontal = horizontal ?? Random.value > 0.5f;
            PlaceSupport(receiver, isHorizontal);
        }

        public void PlaceSupport(WeightReceiver receiver, bool horizontal)
        {
            if (receiver == null) return;
            
            if (_maxSupportsForReceiver.TryGetValue(receiver.Data.Id, out int maxCount) &&
                receiver.Data.AttachedSupportIds.Count >= maxCount)
            {
                UpgradeRandomSupportMaterial(receiver);
                return;
            }
            
            int startPointFailStreak = 0;

            for (int attempt = 0; attempt < 500; attempt++)
            {
                if (receiver == null)
                    break;

                Transform parentRoot = receiver.Transform;
                int generation = 0;
                bool isFromSupport;
                float parentAngle = 0f;
                int parentId = -1;

                bool wantFromSupport = receiver.Data.AttachedSupportIds.Count > 0 && Random.value < 0.2f;

                Vector2 start = wantFromSupport
                    ? TryGetPointFromExistingSupport(receiver, out parentRoot, out parentAngle, out generation,
                        out parentId)
                    : Vector2.zero;

                isFromSupport = start != Vector2.zero;

                if (!isFromSupport)
                {
                    start = GetRandomPointOnReceiver(receiver);
                    parentRoot = receiver.Transform;
                    generation = 0;
                    parentId = -1;
                }

                if (start == Vector2.zero || IsTooCloseToExistingStart(start))
                {
                    startPointFailStreak++;
                    if (startPointFailStreak > 20)
                    {
                        MarkReceiverAtCapacity(receiver);
                        return;
                    }
                    continue;
                }

                startPointFailStreak = 0;

                // НОВЕ: генеруємо кут повністю в межах діапазону (не тільки два краї), максимально рандомно
                float newAngle = horizontal
                    ? (Random.value > 0.5f ? -1f : 1f) *
                    Random.Range(_config.minHorizontalAngle, _config.maxHorizontalAngle) - 90f
                    : (Random.value > 0.5f ? -1f : 1f) *
                    Random.Range(_config.minVerticalAngle, _config.maxVerticalAngle) - 90f;

                if (isFromSupport)
                {
                    float diff = Mathf.Abs(Mathf.DeltaAngle(newAngle, parentAngle));
                    if (diff < _config.minAngleDifference)
                    {
                        float sign = Mathf.DeltaAngle(parentAngle, newAngle) >= 0 ? 1 : -1;
                        newAngle = parentAngle + (sign * _config.minAngleDifference);
                    }
                }

                // НОВЕ: перевіряємо кут відносно ВСІХ вже прикріплених опор цього рецептора, не тільки батьківської
                if (!IsAngleDistinctEnough(receiver, newAngle))
                    continue;

                Vector2 dir = new Vector2(Mathf.Cos(newAngle * Mathf.Deg2Rad), Mathf.Sin(newAngle * Mathf.Deg2Rad));

                float progressT = Mathf.Clamp01(receiver.Data.AttachedSupportIds.Count /
                                                (float)_config.longSupportRoomCountThreshold);
                float preferredMaxLength = Mathf.Lerp(_config.maxLength, _config.minLength, progressT);
                float currentMaxDist = horizontal ? (preferredMaxLength / 3f) : preferredMaxLength;

                Vector2 actualEnd = PerformPhysicsRaycast(start, dir, currentMaxDist, parentRoot);

                if (actualEnd == Vector2.zero) continue;

                float currentLength = Vector2.Distance(start, actualEnd);
                if (currentLength < _config.minLength ||
                    !ValidateConstraints(start, actualEnd, currentMaxDist, isFromSupport, horizontal))
                    continue;

                float currentThickness = Mathf.Lerp(_config.maxThickness, _config.minThickness, progressT);

                if (isFromSupport && _activeSupports.TryGetValue(parentId, out var parentSupportData))
                    currentThickness = Mathf.Min(currentThickness, parentSupportData.Thickness);

                int localId = GetNextLocalId(receiver.Data.Id);
                int compositeId = (receiver.Data.Id * 1000) + localId;

                var finalData = new SupportData(compositeId, start, actualEnd, receiver.Data.Id, generation,
                    currentThickness);

                int levelIndex = Mathf.RoundToInt(progressT * (_config.materialLevels.Count - 1));
                var material = _config.GetLevel(levelIndex);
                var instance = _factory.Create(finalData, material);

                _activeSupports[compositeId] = finalData;
                _activeInstances[compositeId] = instance;
                _supportMaterialLevel[compositeId] = levelIndex; // НОВЕ
                receiver.Data.AttachedSupportIds.Add(compositeId);

                AddUsedStartPoint(start);
                return;
            }

            MarkReceiverAtCapacity(receiver);
            UpgradeRandomSupportMaterial(receiver);
        }

        private bool IsAngleDistinctEnough(WeightReceiver receiver, float newAngle)
        {
            foreach (var id in receiver.Data.AttachedSupportIds)
            {
                if (!_activeSupports.TryGetValue(id, out var data)) continue;

                Vector2 dir = data.End - data.Start;
                float existingAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                if (Mathf.Abs(Mathf.DeltaAngle(newAngle, existingAngle)) < _config.minAngleDifference)
                    return false;
            }

            return true;
        }

        private void MarkReceiverAtCapacity(WeightReceiver receiver)
        {
            _maxSupportsForReceiver[receiver.Data.Id] = receiver.Data.AttachedSupportIds.Count;
        }

        private void UpgradeRandomSupportMaterial(WeightReceiver receiver)
        {
            var ids = receiver.Data.AttachedSupportIds;
            if (ids.Count == 0) return;

            int targetId = -1;
            int lowestLevel = int.MaxValue;

            foreach (var id in ids)
            {
                int level = _supportMaterialLevel.TryGetValue(id, out var l) ? l : 0;
                if (level >= _config.materialLevels.Count - 1) continue;

                if (level < lowestLevel)
                {
                    lowestLevel = level;
                    targetId = id;
                }
            }

            if (targetId == -1) return;

            int newLevel = lowestLevel + 1;
            _supportMaterialLevel[targetId] = newLevel;

            var newMaterial = _config.GetLevel(newLevel);

            if (_activeInstances.TryGetValue(targetId, out var instance))
                instance.UpgradeMaterial(newMaterial);
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

            var firstReceiver = _roomSpawner.FirstReceiver;
            if (firstReceiver == null) return Vector2.zero;

            float groundY = firstReceiver.Transform.position.y - firstReceiver.Data.Size.y / 2f - _config.maxLength;
            if (dir.y < 0)
            {
                float t = (groundY - start.y) / dir.y;
                if (t > 0 && t < dynamicMaxDist)
                {
                    Vector2 fallbackPoint = start + dir * t;

                    // НОВЕ: перевіряємо, що обчислена точка дійсно порожня,
                    // а не всередині якоїсь чужої кімнати
                    var overlap = Physics2D.OverlapPoint(fallbackPoint, _config.maskToCollide);
                    if (overlap != null && (ignoreRoot == null || !overlap.transform.IsChildOf(ignoreRoot)))
                        return Vector2.zero; // там щось є — не ставимо опору "в стіну"/кімнату

                    return fallbackPoint;
                }
            }

            return Vector2.zero;
        }

        private Vector2 TryGetPointFromExistingSupport(WeightReceiver receiver, out Transform parentRoot,
            out float parentAngle, out int nextGeneration, out int parentId)
        {
            parentRoot = receiver.Transform;
            parentAngle = 0f;
            nextGeneration = 0;
            parentId = -1;

            if (receiver.Data.AttachedSupportIds.Count == 0) return Vector2.zero;

            int randomIndex = Random.Range(0, receiver.Data.AttachedSupportIds.Count);
            int candidateId = receiver.Data.AttachedSupportIds[randomIndex];

            if (!_activeSupports.TryGetValue(candidateId, out var parentData)) return Vector2.zero;

            float spawnChance = Mathf.Pow(0.4f, parentData.Generation);
            if (Random.value > spawnChance) return Vector2.zero;

            parentId = candidateId;
            nextGeneration = parentData.Generation + 1;
            Vector2 parentDir = parentData.End - parentData.Start;
            parentAngle = Mathf.Atan2(parentDir.y, parentDir.x) * Mathf.Rad2Deg;

            if (_activeInstances.TryGetValue(candidateId, out var parentInstance))
                parentRoot = parentInstance.transform;

            return parentData.GetPointOnLine(Random.Range(0.3f, 0.8f));
        }

        private bool ValidateConstraints(Vector2 start, Vector2 end, float currentMaxDist, bool isFromSupport,
            bool isHorizontalMode)
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

        private Vector2 GetRandomPointOnReceiver(WeightReceiver receiver)
        {
            var box = receiver.BoxCollider;
            if (box == null) return Vector2.zero;

            Bounds bounds = box.bounds;

            float minX = bounds.min.x;
            float maxX = bounds.max.x;
            float y = bounds.min.y;

            float safeMin = minX + 0.4f;
            float safeMax = maxX - 0.4f;

            if (safeMin >= safeMax) return Vector2.zero;

            for (int i = 0; i < 30; i++)
            {
                float testX = Random.Range(safeMin, safeMax);
                Vector2 point = new Vector2(testX, y);

                Vector2 insideCheck = new Vector2(testX, y + 0.05f);
                var insideHit = Physics2D.OverlapPoint(insideCheck, _roomsLayerMask);
                if (insideHit != box) continue;

                Vector2 checkPoint = new Vector2(testX, y - 0.05f);
                var belowHit = Physics2D.OverlapPoint(checkPoint, _roomsLayerMask);
                if (belowHit != null && belowHit != box) continue;

                // НОВЕ: перевіряємо, чи немає поруч чужого колайдера (кут/стик кімнат)
                if (IsTooCloseToOtherRoom(point, box)) continue;

                return point;
            }

            return Vector2.zero;
        }

        private bool IsTooCloseToOtherRoom(Vector2 point, Collider2D ownCollider)
        {
            var hits = Physics2D.OverlapCircleAll(point, _config.supportEdgeMargin, _roomsLayerMask);
            foreach (var hit in hits)
            {
                if (hit != ownCollider)
                    return true; // поруч є чужа кімната — це кут/стик, точку не використовуємо
            }
            return false;
        }

        private bool IsTooCloseToExistingStart(Vector2 point)
        {
            float minDistance = Mathf.Max(0.5f, _config.supportEdgeMargin);
            for (int i = 0; i < _usedStartPoints.Count; i++)
            {
                if (Vector2.Distance(_usedStartPoints[i], point) < minDistance)
                    return true;
            }

            return false;
        }

        private void AddUsedStartPoint(Vector2 point)
        {
            _usedStartPoints.Add(point);
        }

        public void RemoveSupport(int id)
        {
            _activeSupports.Remove(id);
            _supportMaterialLevel.Remove(id);

            if (_activeInstances.TryGetValue(id, out var instance))
            {
                instance.Dispose();
                _activeInstances.Remove(id);
            }

            int receiverId = id / 1000;
            _maxSupportsForReceiver.Remove(receiverId);
        }

        private int GetNextLocalId(int receiverId)
        {
            if (!_receiverSupportCounters.ContainsKey(receiverId)) _receiverSupportCounters[receiverId] = 1;
            return _receiverSupportCounters[receiverId]++;
        }

        public void ClearAll()
        {
            foreach (var instance in _activeInstances.Values)
                instance.Dispose();

            _activeSupports.Clear();
            _activeInstances.Clear();
            _receiverSupportCounters.Clear();
            _usedStartPoints.Clear();
            _maxSupportsForReceiver.Clear();
            _supportMaterialLevel.Clear();
        }

        public void Dispose() => ClearAll();
    }
}