using System.Collections.Generic;
using _Game.CodeBase.Features.BuildingModule.Scripts.RoomsAndObjects;
using _Game.CodeBase.Features.BuildingModule.Scripts.RoomsAndObjects.Data;
using _Game.CodeBase.Features.BuildingModule.Scripts.Supports;
using UnityEngine;
using Zenject;

namespace _Game.CodeBase.Features.BuildingModule.Scripts.Weight
{
    public class WeightDistributionSystem
    {
        private readonly WeightDistributionConfig _config;
        private readonly RoomSpawner _roomSpawner;
        private readonly SupportsGenerator _supportsGenerator;
        private readonly LayerMask _roomsLayerMask;
        private readonly SignalBus _signalBus;

        private static readonly Collider2D[] OverlapBuffer = new Collider2D[16];
        private readonly List<WeightReceiver> _sortedReceivers = new();
        private readonly Dictionary<int, float> _incomingLoad = new();
        private readonly List<WeightReceiver> _sideReceiversBuffer = new();
        private readonly ContactFilter2D _roomsContactFilter;

        public WeightDistributionSystem(WeightDistributionConfig config, RoomSpawner roomSpawner,
            SupportsGenerator supportsGenerator, LayerMask roomsLayerMask, SignalBus signalBus)
        {
            _config = config;
            _roomSpawner = roomSpawner;
            _supportsGenerator = supportsGenerator;
            _roomsLayerMask = roomsLayerMask;
            _signalBus = signalBus;

            _roomsContactFilter = new ContactFilter2D();
            _roomsContactFilter.SetLayerMask(_roomsLayerMask);
            _roomsContactFilter.useTriggers = false;

            _signalBus.Subscribe<RoomSpawnedSignal>(OnRoomSpawned);
            _signalBus.Subscribe<SupportPlacedSignal>(OnSupportPlaced);
        }

        private void OnRoomSpawned(RoomSpawnedSignal signal)
        {
            RecalculateAll();
        }

        private void OnSupportPlaced(SupportPlacedSignal signal)
        {
            RecalculateAll();
        }

        public void RecalculateAll()
        {
            var receivers = _roomSpawner.WeightReceivers;
            if (receivers.Count == 0) return;

            _sortedReceivers.Clear();
            _sortedReceivers.AddRange(receivers);
            _sortedReceivers.Sort((a, b) => b.Transform.position.y.CompareTo(a.Transform.position.y));

            _incomingLoad.Clear();

            foreach (var receiver in _sortedReceivers)
            {
                if (receiver.BoxCollider == null) continue;

                float incoming = _incomingLoad.TryGetValue(receiver.Data.Id, out var v) ? v : 0f;
                receiver.Data.SetReceivedLoad(incoming);

                float totalWeight = receiver.Data.TotalWeight + incoming;
                
                if (receiver.IsInfiniteAnchor)
                {
                    receiver.Data.SetNotStabilizedLoad(0f);
                    continue;
                }

                if (totalWeight <= _config.minWeightToDistribute)
                {
                    receiver.Data.SetNotStabilizedLoad(0f);
                    continue;
                }

                float transferred = 0f;

                float directDownFraction = DistributeDirectlyDown(receiver, totalWeight);
                float directDownWeight = totalWeight * directDownFraction;
                transferred += directDownWeight;

                float remaining = totalWeight - directDownWeight;

                if (remaining > _config.minWeightToDistribute)
                {
                    float toSupports;

                    if (directDownFraction <= 0.0001f)
                    {
                        _sideReceiversBuffer.Clear();
                        FindSideReceivers(receiver, _sideReceiversBuffer);

                        if (_sideReceiversBuffer.Count > 0)
                        {
                            float sideWeight = remaining * _config.sideTransferFraction;
                            float perSide = sideWeight / _sideReceiversBuffer.Count;

                            foreach (var side in _sideReceiversBuffer)
                                AddIncoming(side.Data.Id, perSide);

                            transferred += sideWeight;
                            toSupports = remaining - sideWeight;
                        }
                        else
                        {
                            toSupports = remaining;
                        }
                    }
                    else
                    {
                        toSupports = remaining;
                    }

                    transferred += DistributeToSupports(receiver, toSupports);
                }

                receiver.Data.SetNotStabilizedLoad(totalWeight - transferred);
            }
        }

        private float DistributeDirectlyDown(WeightReceiver receiver, float weightToDistribute)
        {
            Bounds bounds = receiver.BoxCollider.bounds;
            float thisMinX = bounds.min.x;
            float thisMaxX = bounds.max.x;
            float thisWidth = bounds.size.x;

            if (thisWidth <= 0f) return 0f;

            Vector2 checkCenter = new Vector2(bounds.center.x, bounds.min.y - _config.detectionTolerance);
            Vector2 checkSize = new Vector2(thisWidth, _config.detectionTolerance * 2f);

            int hitCount = Physics2D.OverlapBox(checkCenter, checkSize, 0f, _roomsContactFilter, OverlapBuffer);

            float coveredWidth = 0f;
            var belowShares = new List<(WeightReceiver receiver, float overlapWidth)>();

            for (int i = 0; i < hitCount; i++)
            {
                var col = OverlapBuffer[i];
                if (col == null || col == receiver.BoxCollider) continue;

                var otherReceiver = col.GetComponent<WeightReceiver>();
                if (otherReceiver == null) continue;

                var otherBounds = col.bounds;
                float overlapMinX = Mathf.Max(thisMinX, otherBounds.min.x);
                float overlapMaxX = Mathf.Min(thisMaxX, otherBounds.max.x);
                float overlapWidth = Mathf.Max(0f, overlapMaxX - overlapMinX);

                if (overlapWidth <= 0f) continue;

                belowShares.Add((otherReceiver, overlapWidth));
                coveredWidth += overlapWidth;
            }

            if (belowShares.Count == 0 || coveredWidth <= 0f) return 0f;

            coveredWidth = Mathf.Min(coveredWidth, thisWidth);
            float directDownFraction = Mathf.Clamp01(coveredWidth / thisWidth);

            float directDownWeight = weightToDistribute * directDownFraction;

            foreach (var (belowReceiver, overlapWidth) in belowShares)
            {
                float share = directDownWeight * (overlapWidth / coveredWidth);
                AddIncoming(belowReceiver.Data.Id, share);
            }

            return directDownFraction;
        }

        private float DistributeToSupports(WeightReceiver receiver, float weightToDistribute)
        {
            var ids = receiver.Data.AttachedSupportIds;
            if (ids.Count == 0 || weightToDistribute <= 0f) return 0f;

            float remainingToDistribute = weightToDistribute;
            float transferred = 0f;

            foreach (var supportId in ids)
            {
                if (remainingToDistribute <= 0f) break;

                if (!_supportsGenerator.TryGetSupportData(supportId, out var supportData)) continue;
                if (!_supportsGenerator.TryGetSupportMaxLoad(supportId, out float maxLoad)) continue;

                float amountThroughThisSupport = Mathf.Min(remainingToDistribute, maxLoad);
                if (amountThroughThisSupport <= 0f) continue;

                var hit = Physics2D.OverlapPoint(supportData.End, _roomsLayerMask);

                if (hit == null)
                {
                    transferred += amountThroughThisSupport;
                    remainingToDistribute -= amountThroughThisSupport;
                    continue;
                }

                var targetReceiver = hit.GetComponent<WeightReceiver>();
                if (targetReceiver == null || targetReceiver == receiver)
                {
                    transferred += amountThroughThisSupport;
                    remainingToDistribute -= amountThroughThisSupport;
                    continue;
                }

                AddIncoming(targetReceiver.Data.Id, amountThroughThisSupport);
                transferred += amountThroughThisSupport;
                remainingToDistribute -= amountThroughThisSupport;
            }

            return transferred;
        }

        private void FindSideReceivers(WeightReceiver receiver, List<WeightReceiver> result)
        {
            Bounds bounds = receiver.BoxCollider.bounds;

            Vector2 leftCenter = new Vector2(bounds.min.x - _config.detectionTolerance, bounds.center.y);
            Vector2 rightCenter = new Vector2(bounds.max.x + _config.detectionTolerance, bounds.center.y);
            Vector2 size = new Vector2(_config.detectionTolerance * 2f, bounds.size.y);

            CollectReceiversAt(leftCenter, size, receiver, result);
            CollectReceiversAt(rightCenter, size, receiver, result);
        }

        private void CollectReceiversAt(Vector2 center, Vector2 size, WeightReceiver self, List<WeightReceiver> result)
        {
            int hitCount = Physics2D.OverlapBox(center, size, 0f, _roomsContactFilter, OverlapBuffer);

            for (int i = 0; i < hitCount; i++)
            {
                var col = OverlapBuffer[i];
                if (col == null || col == self.BoxCollider) continue;

                var other = col.GetComponent<WeightReceiver>();
                if (other != null && !result.Contains(other))
                    result.Add(other);
            }
        }

        private void AddIncoming(int receiverId, float amount)
        {
            _incomingLoad[receiverId] = (_incomingLoad.TryGetValue(receiverId, out var v) ? v : 0f) + amount;
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<RoomSpawnedSignal>(OnRoomSpawned);
            _signalBus.Unsubscribe<SupportPlacedSignal>(OnSupportPlaced);
        }
    }
}