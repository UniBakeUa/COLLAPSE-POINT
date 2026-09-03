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
        private enum EdgeType { Down, Support, Side, Up }

        private struct Edge
        {
            public int To;
            public int Cost;
            public EdgeType Type;
            public float SupportMaxLoad;
        }

        public struct WeightFlowDebugInfo
        {
            public Vector2 FromPos;
            public Vector2 ToPos;
            public Color Color;
        }

        public List<WeightFlowDebugInfo> ActiveFlows { get; } = new();

        private readonly WeightDistributionConfig _config;
        private readonly RoomSpawner _roomSpawner;
        private readonly SupportsGenerator _supportsGenerator;
        private readonly LayerMask _roomsLayerMask;
        private readonly SignalBus _signalBus;

        private static readonly Collider2D[] OverlapBuffer = new Collider2D[16];
        private readonly ContactFilter2D _roomsContactFilter;

        private readonly Dictionary<int, List<Edge>> _forwardEdges = new();
        private readonly Dictionary<int, List<Edge>> _reverseEdges = new();

        private readonly Dictionary<int, int> _nextHop = new();
        private readonly Dictionary<int, Edge> _nextHopEdge = new();
        private readonly Dictionary<int, int> _cost = new();
        private readonly List<int> _finalizeOrder = new();
        private readonly Dictionary<int, WeightReceiver> _receiverById = new();

        // ДОДАНО: скільки вже "продавлено" через primary-шлях кожного вузла, і скільки max можна (з толерансом)
        private readonly Dictionary<int, float> _primaryUsed = new();
        private readonly Dictionary<int, float> _primaryCapacity = new();

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

        private void OnRoomSpawned(RoomSpawnedSignal signal) => RecalculateAll();
        private void OnSupportPlaced(SupportPlacedSignal signal) => RecalculateAll();

        public void RecalculateAll()
        {
            var receivers = _roomSpawner.WeightReceivers;
            if (receivers.Count == 0) return;

            BuildGraph(receivers);
            BuildPathsToNearestAnchor(receivers);

            var stress = PropagateWeightPrimary(); // ЗМІНЕНО: тепер повертає початковий "стрес" замість одразу писати NotStabilizedLoad
            DiffuseStress(stress);                 // ДОДАНО: розтікання надлишку по сусідах
        }

        private void BuildGraph(IReadOnlyList<WeightReceiver> receivers)
        {
            _forwardEdges.Clear();
            _receiverById.Clear();

            foreach (var r in receivers)
            {
                if (r.BoxCollider == null) continue;
                _receiverById[r.Data.Id] = r;
                _forwardEdges[r.Data.Id] = new List<Edge>();
            }

            foreach (var r in receivers)
            {
                if (r.BoxCollider == null) continue;

                Bounds bounds = r.BoxCollider.bounds;

                Vector2 belowCenter = new Vector2(bounds.center.x, bounds.min.y - _config.detectionTolerance);
                Vector2 belowSize = new Vector2(bounds.size.x * 0.8f, _config.detectionTolerance * 2f);
                AddDownNeighborsAt(r, belowCenter, belowSize);

                foreach (var supportId in r.Data.AttachedSupportIds)
                {
                    if (!_supportsGenerator.TryGetSupportData(supportId, out var data)) continue;
                    if (!_supportsGenerator.TryGetSupportMaxLoad(supportId, out float maxLoad)) continue;
                    if (!_supportsGenerator.TryGetSupportInstance(supportId, out var supportInstance)) continue;

                    var target = supportInstance.TargetWeightReceiver;
                    if (target == null || target == r) continue;

                    AddForwardEdge(r.Data.Id, target.Data.Id, 0, EdgeType.Support, maxLoad);
                }

                Vector2 leftCenter = new Vector2(bounds.min.x - _config.detectionTolerance, bounds.center.y);
                Vector2 rightCenter = new Vector2(bounds.max.x + _config.detectionTolerance, bounds.center.y);
                Vector2 sideSize = new Vector2(_config.detectionTolerance * 2f, bounds.size.y * 0.8f);
                AddSideNeighborsAt(r, leftCenter, sideSize);
                AddSideNeighborsAt(r, rightCenter, sideSize);
            }
        }

        private void AddDownNeighborsAt(WeightReceiver self, Vector2 center, Vector2 size)
        {
            int hitCount = Physics2D.OverlapBox(center, size, 0f, _roomsContactFilter, OverlapBuffer);
            for (int i = 0; i < hitCount; i++)
            {
                var col = OverlapBuffer[i];
                if (col == null || col == self.BoxCollider) continue;
                var other = col.GetComponent<WeightReceiver>();
                if (other == null) continue;

                AddForwardEdge(self.Data.Id, other.Data.Id, 0, EdgeType.Down, 0f);

                if (_config.enableUpwardTransfer)
                    AddForwardEdge(other.Data.Id, self.Data.Id, 1, EdgeType.Up, 0f);
            }
        }

        private void AddSideNeighborsAt(WeightReceiver self, Vector2 center, Vector2 size)
        {
            int hitCount = Physics2D.OverlapBox(center, size, 0f, _roomsContactFilter, OverlapBuffer);
            for (int i = 0; i < hitCount; i++)
            {
                var col = OverlapBuffer[i];
                if (col == null || col == self.BoxCollider) continue;
                var other = col.GetComponent<WeightReceiver>();
                if (other == null) continue;
                AddForwardEdge(self.Data.Id, other.Data.Id, 1, EdgeType.Side, 0f);
                AddForwardEdge(other.Data.Id, self.Data.Id, 1, EdgeType.Side, 0f);
            }
        }

        private void AddForwardEdge(int from, int to, int cost, EdgeType type, float supportMaxLoad)
        {
            if (!_forwardEdges.TryGetValue(from, out var list)) return;
            list.Add(new Edge { To = to, Cost = cost, Type = type, SupportMaxLoad = supportMaxLoad });
        }

        private void BuildPathsToNearestAnchor(IReadOnlyList<WeightReceiver> receivers)
        {
            _reverseEdges.Clear();
            _nextHop.Clear();
            _nextHopEdge.Clear();
            _cost.Clear();
            _finalizeOrder.Clear();

            foreach (var kv in _forwardEdges)
            {
                foreach (var edge in kv.Value)
                {
                    if (!_reverseEdges.TryGetValue(edge.To, out var list))
                    {
                        list = new List<Edge>();
                        _reverseEdges[edge.To] = list;
                    }
                    list.Add(new Edge { To = kv.Key, Cost = edge.Cost, Type = edge.Type, SupportMaxLoad = edge.SupportMaxLoad });
                }
            }

            var finalized = new HashSet<int>();
            var deque = new LinkedList<int>();

            foreach (var r in receivers)
            {
                if (r.BoxCollider == null || !r.IsInfiniteAnchor) continue;
                _cost[r.Data.Id] = 0;
                _nextHop[r.Data.Id] = r.Data.Id;
                deque.AddFirst(r.Data.Id);
            }

            while (deque.Count > 0)
            {
                int cur = deque.First.Value;
                deque.RemoveFirst();

                if (finalized.Contains(cur)) continue;
                finalized.Add(cur);
                _finalizeOrder.Add(cur);

                if (!_reverseEdges.TryGetValue(cur, out var incoming)) continue;

                foreach (var edge in incoming)
                {
                    int from = edge.To;
                    if (finalized.Contains(from)) continue;

                    int newCost = _cost[cur] + edge.Cost;
                    if (!_cost.TryGetValue(from, out int existingCost) || newCost < existingCost)
                    {
                        _cost[from] = newCost;
                        _nextHop[from] = cur;
                        _nextHopEdge[from] = new Edge { To = cur, Cost = edge.Cost, Type = edge.Type, SupportMaxLoad = edge.SupportMaxLoad };

                        if (edge.Cost == 0) deque.AddFirst(from);
                        else deque.AddLast(from);
                    }
                }
            }
        }

        /// <summary>
        /// Основний прохід: кожен вузол шле вагу до якоря через свій найкоротший шлях,
        /// з ДОПУСКОМ перевантаження (overloadTolerance) на support-ребрах.
        /// Все, що не влазить навіть з допуском, повертається як "стрес" для подальшого розтікання.
        /// </summary>
        private Dictionary<int, float> PropagateWeightPrimary()
        {
            _primaryUsed.Clear();
            _primaryCapacity.Clear();

            var incoming = new Dictionary<int, float>();
            var stress = new Dictionary<int, float>(); // ДОДАНО
            ActiveFlows.Clear();

            for (int i = _finalizeOrder.Count - 1; i >= 0; i--)
            {
                int id = _finalizeOrder[i];
                if (!_receiverById.TryGetValue(id, out var receiver)) continue;

                float incomingLoad = incoming.TryGetValue(id, out var v) ? v : 0f;
                receiver.Data.SetReceivedLoad(incomingLoad);

                if (receiver.IsInfiniteAnchor)
                {
                    receiver.Data.SetNotStabilizedLoad(0f);
                    continue;
                }

                float totalWeight = receiver.Data.BaseWeight + incomingLoad;

                List<Edge> validEdges = new List<Edge>();
                if (_forwardEdges.TryGetValue(id, out var outEdges) && _cost.TryGetValue(id, out int currentCost))
                {
                    foreach (var edge in outEdges)
                    {
                        if (_cost.TryGetValue(edge.To, out int neighborCost) && neighborCost + edge.Cost == currentCost)
                            validEdges.Add(edge);
                    }
                }

                if (validEdges.Count == 0)
                {
                    stress[id] = totalWeight; // ЗМІНЕНО: не одразу NotStabilizedLoad, а в стрес на розтікання
                    _primaryCapacity[id] = 0f;
                    _primaryUsed[id] = 0f;
                    continue;
                }

                float weightPerEdge = totalWeight / validEdges.Count;
                float totalTransferred = 0f;
                float totalCapacity = 0f; // ДОДАНО

                foreach (var edge in validEdges)
                {
                    float capacity = edge.Type switch
                    {
                        EdgeType.Support => edge.SupportMaxLoad * (1f + _config.overloadTolerance), // ЗМІНЕНО: допуск
                        EdgeType.Side => weightPerEdge * _config.sideTransferFraction,
                        EdgeType.Up => weightPerEdge * _config.upTransferFraction,
                        EdgeType.Down => float.MaxValue,
                        _ => float.MaxValue
                    };

                    totalCapacity += capacity == float.MaxValue ? weightPerEdge : capacity; // ДОДАНО

                    float transferredToThisEdge = Mathf.Min(weightPerEdge, capacity);

                    if (transferredToThisEdge >= _config.minWeightToDistribute / validEdges.Count)
                    {
                        incoming[edge.To] = (incoming.TryGetValue(edge.To, out var pv) ? pv : 0f) + transferredToThisEdge;
                        totalTransferred += transferredToThisEdge;

                        if (_receiverById.TryGetValue(edge.To, out var toRec) && toRec.BoxCollider != null)
                        {
                            Color lineColor = edge.Type switch
                            {
                                EdgeType.Down => Color.green,
                                EdgeType.Support => Color.blue,
                                EdgeType.Side => Color.yellow,
                                EdgeType.Up => Color.red,
                                _ => Color.white
                            };

                            ActiveFlows.Add(new WeightFlowDebugInfo
                            {
                                FromPos = receiver.BoxCollider.bounds.center,
                                ToPos = toRec.BoxCollider.bounds.center,
                                Color = lineColor
                            });
                        }
                    }
                }

                _primaryCapacity[id] = totalCapacity; // ДОДАНО: скільки максимум цей вузол міг би пропустити (з допуском)
                _primaryUsed[id] = totalTransferred;   // ДОДАНО: скільки реально пропустив

                float excess = totalWeight - totalTransferred;
                if (excess > _config.minWeightToDistribute)
                    stress[id] = excess; // ЗМІНЕНО: замість NotStabilizedLoad — у стрес
                else
                    receiver.Data.SetNotStabilizedLoad(0f);
            }

            return stress;
        }

        /// <summary>
        /// Розтікання надлишку ("стресу") по сусідах (Side-ребра). Кожен вузол спершу пробує
        /// протиснути стрес через свій ВЖЕ ВИКОРИСТАНИЙ первинний шлях (якщо там лишився запас
        /// капасіті з допуском), а решту — рівномірно роздає бічним сусідам. Ітерується кілька
        /// разів (config.maxOverloadIterations), поки стрес не розчиниться або не "застрягне" —
        /// застрягла частина стає фінальним NotStabilizedLoad (візуально — тріщини/колапс).
        /// </summary>
        private void DiffuseStress(Dictionary<int, float> stress)
        {
            var finalStuck = new Dictionary<int, float>();

            for (int iteration = 0; iteration < _config.maxOverloadIterations; iteration++)
            {
                if (stress.Count == 0) break;

                var nextStress = new Dictionary<int, float>();

                foreach (var (id, amount) in stress)
                {
                    if (amount <= _config.minWeightToDistribute) continue;
                    if (!_receiverById.TryGetValue(id, out var receiver)) continue;

                    float remaining = amount;

                    // 1) пробуємо протиснути через власний первинний шлях, якщо там лишився запас
                    if (_primaryCapacity.TryGetValue(id, out float capacity) && _primaryUsed.TryGetValue(id, out float used))
                    {
                        float spare = Mathf.Max(0f, capacity - used);
                        float pushed = Mathf.Min(remaining, spare);

                        if (pushed > 0f)
                        {
                            _primaryUsed[id] = used + pushed;
                            remaining -= pushed;
                        }
                    }

                    if (remaining <= _config.minWeightToDistribute) continue;

                    // 2) решту — вбік, сусідам (ліворуч/праворуч)
                    var sideNeighbors = GetSideNeighbors(id);

                    if (sideNeighbors.Count == 0)
                    {
                        finalStuck[id] = (finalStuck.TryGetValue(id, out var fv) ? fv : 0f) + remaining;
                        continue;
                    }

                    float share = remaining / sideNeighbors.Count;
                    foreach (var neighborId in sideNeighbors)
                        nextStress[neighborId] = (nextStress.TryGetValue(neighborId, out var nv) ? nv : 0f) + share;

                    // ДОДАНО: дебаг-лінії стресу (фіолетові) — видно, куди "тріщина" розповзається
                    if (_receiverById.TryGetValue(id, out var fromRec) && fromRec.BoxCollider != null)
                    {
                        foreach (var neighborId in sideNeighbors)
                        {
                            if (_receiverById.TryGetValue(neighborId, out var toRec) && toRec.BoxCollider != null)
                            {
                                ActiveFlows.Add(new WeightFlowDebugInfo
                                {
                                    FromPos = fromRec.BoxCollider.bounds.center,
                                    ToPos = toRec.BoxCollider.bounds.center,
                                    Color = Color.magenta
                                });
                            }
                        }
                    }
                }

                stress = nextStress;
            }

            // Все, що лишилось у stress після останньої ітерації і не потрапило в finalStuck — теж застрягло
            foreach (var (id, amount) in stress)
            {
                if (amount <= _config.minWeightToDistribute) continue;
                finalStuck[id] = (finalStuck.TryGetValue(id, out var fv) ? fv : 0f) + amount;
            }

            foreach (var (id, amount) in finalStuck)
            {
                if (_receiverById.TryGetValue(id, out var receiver))
                    receiver.Data.SetNotStabilizedLoad(amount);
            }
        }

        private List<int> GetSideNeighbors(int id)
        {
            var result = new List<int>();
            if (!_forwardEdges.TryGetValue(id, out var edges)) return result;

            foreach (var edge in edges)
            {
                if (edge.Type == EdgeType.Side && !result.Contains(edge.To))
                {
                    if (_receiverById.TryGetValue(edge.To, out var neighbor) && !neighbor.IsInfiniteAnchor)
                        result.Add(edge.To);
                }
            }

            return result;
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<RoomSpawnedSignal>(OnRoomSpawned);
            _signalBus.Unsubscribe<SupportPlacedSignal>(OnSupportPlaced);
        }
    }
}