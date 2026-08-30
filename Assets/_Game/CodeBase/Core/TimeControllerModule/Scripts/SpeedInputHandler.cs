using UnityEngine.InputSystem;
using Zenject;

namespace _Game.CodeBase.Core.TimeControllerModule.Scripts
{
    public class SpeedInputHandler : ITickable
    {
        private readonly SpeedController _speedController;

        public SpeedInputHandler(SpeedController speedController)
        {
            _speedController = speedController;
        }

        public void Tick()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.digit1Key.wasPressedThisFrame)
                _speedController.ChangeSpeed(0);
            else if (keyboard.digit2Key.wasPressedThisFrame)
                _speedController.ChangeSpeed(1);
            else if (keyboard.digit3Key.wasPressedThisFrame)
                _speedController.ChangeSpeed(2);
        }
    }
}