using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace _Game.CodeBase.Core.TimeControllerModule.Scripts
{
    public class ChangeSpeedButton : MonoBehaviour
    {
        [SerializeField] private int speedChangeTo;
        [Inject] private SpeedController _speedController;
        
        private Button _button;
        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(ChangeSpeed);
        }

        private void ChangeSpeed()
        {
            _speedController.ChangeSpeed(speedChangeTo);
        }
    }
}