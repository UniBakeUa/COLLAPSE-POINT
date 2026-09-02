using System;
using _Game.CodeBase.Features.BuildingModule.Scripts.Data;
using UnityEngine;
using Zenject;
using _Game.CodeBase.Features.BuildingModule.Scripts.Weight;

namespace _Game.CodeBase.Features.BuildingModule.Scripts
{
    [RequireComponent(typeof(LineRenderer))]
    [RequireComponent(typeof(EdgeCollider2D))]
    public class Support : MonoBehaviour, IPoolable<IMemoryPool>, IDisposable
    {
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private EdgeCollider2D _collider;

        private IMemoryPool _pool;
        private Vector3 _startPoint;
        private Vector3 _endPoint;

        public Vector3 Start => _startPoint;
        public Vector3 End => _endPoint;

        private WeightReceiver _targetWeightReceiver;
        public WeightReceiver TargetWeightReceiver => _targetWeightReceiver;
        public void Setup(SupportData data, SupportMaterialLevel material, WeightReceiver targetReceiver)
        {
            _startPoint = data.Start;
            _endPoint = data.End;

            _lineRenderer.positionCount = 2;
            _lineRenderer.SetPosition(0, _startPoint);
            _lineRenderer.SetPosition(1, _endPoint);

            _lineRenderer.startWidth = data.Thickness;
            _lineRenderer.endWidth = data.Thickness;
            _lineRenderer.startColor = material.visualColor;
            _lineRenderer.endColor = material.visualColor;

            _targetWeightReceiver = targetReceiver;
            
            _collider.points = new[]
            {
                (Vector2)transform.InverseTransformPoint(_startPoint),
                (Vector2)transform.InverseTransformPoint(_endPoint)
            };
        }
        public void UpgradeMaterial(SupportMaterialLevel material)
        {
            _lineRenderer.startColor = material.visualColor;
            _lineRenderer.endColor = material.visualColor;
        }
        public Vector3 GetAnchorPoint() => Vector3.Lerp(_startPoint, _endPoint, 0.5f);

        public void OnSpawned(IMemoryPool pool) => _pool = pool;
        public void OnDespawned() => _pool = null;
        public void Dispose() => _pool?.Despawn(this);
    }
}