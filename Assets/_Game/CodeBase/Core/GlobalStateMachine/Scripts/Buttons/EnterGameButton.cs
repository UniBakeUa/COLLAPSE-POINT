using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace _Game.CodeBase.Core.GlobalStateMachine.Scripts.Buttons
{
    public class EnterGameButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        
        private _Game.Core.GlobalStateMachine.Scripts.GlobalStateMachine _stateMachine;
        
        [Inject]
        public void Construct(_Game.Core.GlobalStateMachine.Scripts.GlobalStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }
        private void Awake()
        {
            _button.onClick.AddListener(EnterGame);
        }

        private void EnterGame()
        {
            _stateMachine.EnterPlaying();
        }
    }
}