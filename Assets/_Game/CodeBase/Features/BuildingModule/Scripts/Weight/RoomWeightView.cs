using _Game.CodeBase.Features.BuildingModule.Scripts.RoomsAndObjects;
using _Game.CodeBase.Features.BuildingModule.Scripts.Weight;
using TMPro;
using UnityEngine;
using Zenject;

namespace _Game.CodeBase.Features.BuildingModule.Scripts.UI
{
    public class RoomWeightView : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _weightText;

        private RoomWeightInspector _inspector;
        private WeightReceiver _subscribedReceiver;

        [Inject]
        private void Construct(RoomWeightInspector inspector)
        {
            _inspector = inspector;
        }

        private void OnEnable()
        {
            if (_panel != null) _panel.SetActive(false);
            _inspector.OnSelectionChanged += HandleSelectionChanged;
        }

        private void OnDisable()
        {
            _inspector.OnSelectionChanged -= HandleSelectionChanged;
            UnsubscribeFromReceiver();
        }

        private void HandleSelectionChanged(WeightReceiver receiver)
        {
            UnsubscribeFromReceiver();

            if (receiver == null)
            {
                if (_panel != null) _panel.SetActive(false);
                return;
            }

            _subscribedReceiver = receiver;
            _subscribedReceiver.Data.OnWeightChanged += RefreshText;

            if (_panel != null) _panel.SetActive(true);
            RefreshText();
        }

        private void UnsubscribeFromReceiver()
        {
            if (_subscribedReceiver != null)
                _subscribedReceiver.Data.OnWeightChanged -= RefreshText;

            _subscribedReceiver = null;
        }

        private void RefreshText()
        {
            if (_weightText == null || _subscribedReceiver == null) return;

            var data = _subscribedReceiver.Data;

            _weightText.text =
                $"Room #{data.Id}\n" +
                $"Base weight: {data.BaseWeight:F2}\n" +
                $"Received load: {data.ReceivedLoad:F2}\n" +
                $"Not-stabilized load: {data.NotStabilizedLoad:F2}\n" +
                $"Total: {data.TotalWeight + data.ReceivedLoad:F2}";
        }
    }
}