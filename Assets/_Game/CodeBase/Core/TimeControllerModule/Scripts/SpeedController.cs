using System.Collections.Generic;
using Zenject;

namespace _Game.CodeBase.Core.TimeControllerModule.Scripts
{
    public class SpeedController
    {
        private int _currentSpeed;
        private List<int> _speedCollection;
        private SignalBus  _signalBus;
        
        public int GetCurrentSpeed() => _currentSpeed;
        
        public SpeedController(SpeedConfig speedConfig,  SignalBus signalBus)
        {
            _signalBus = signalBus;
            _speedCollection = speedConfig.speedCollection;
        }

        public void ChangeSpeed(int speed)
        {
            _currentSpeed = _speedCollection[speed];
            _signalBus.Fire(new SpeedChangedSignal() { Speed = _speedCollection[speed] });
        }
    }
}