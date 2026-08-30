using UnityEngine;
using Zenject;

namespace _Game.CodeBase.Core.TimeControllerModule.Scripts
{
    public class MonoBehaivorSpeedController : MonoBehaviour
    {
        [Inject] SignalBus _signalBus;
        private void Awake()
        {
            _signalBus.Subscribe<SpeedChangedSignal>(OnSpeedChanged);
        }

        private void OnSpeedChanged(SpeedChangedSignal signal)
        {
            Time.timeScale = signal.Speed;
        }
        private void OnDestroy()
        {
            _signalBus?.Unsubscribe<SpeedChangedSignal>(OnSpeedChanged);
        }
    }
}