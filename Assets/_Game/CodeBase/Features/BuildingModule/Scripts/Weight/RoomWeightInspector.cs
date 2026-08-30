using System;
using _Game.CodeBase.Features.BuildingModule.Scripts.RoomsAndObjects;
using _Game.CodeBase.Features.BuildingModule.Scripts.Weight;
using _Game.Core.InputSystemModule.Scripts;
using UnityEngine;
using Zenject;

namespace _Game.CodeBase.Features.BuildingModule.Scripts.UI
{
    public class RoomWeightInspector : IInitializable, IDisposable
    {
        private readonly IInputService _inputService;
        private readonly Camera _camera;
        private readonly LayerMask _roomsLayerMask;

        public WeightReceiver SelectedReceiver { get; private set; }
        
        public event Action<WeightReceiver> OnSelectionChanged;

        public RoomWeightInspector(IInputService inputService, Camera camera, LayerMask roomsLayerMask)
        {
            _inputService = inputService;
            _camera = camera;
            _roomsLayerMask = roomsLayerMask;
        }

        public void Initialize()
        {
            _inputService.OnSimpleClick += OnSimpleClick;
        }

        public void Dispose()
        {
            _inputService.OnSimpleClick -= OnSimpleClick;
        }

        private void OnSimpleClick(Vector2 screenPosition)
        {
            if (_camera == null) return;
            Vector2 worldPoint = _camera.ScreenToWorldPoint(screenPosition);
            var hit = Physics2D.OverlapPoint(worldPoint, _roomsLayerMask);

            if (hit == null)
            {
                Deselect();
                return;
            }

            var receiver = hit.GetComponent<WeightReceiver>();
            if (receiver == null)
            {
                Deselect();
                return;
            }

            Select(receiver);
        }

        private void Select(WeightReceiver receiver)
        {
            if (SelectedReceiver == receiver) return;

            SelectedReceiver = receiver;
            OnSelectionChanged?.Invoke(receiver);
        }

        private void Deselect()
        {
            if (SelectedReceiver == null) return;

            SelectedReceiver = null;
            OnSelectionChanged?.Invoke(null);
        }
    }
}