using _Game.Core.InputSystemModule.Scripts;
using UnityEngine;
using Zenject;

namespace _Game.Core.GameplayStateMachine.Scripts.States
{
    public class PlayingState : GameplayStateBase
    {
        [Inject] InputService inputService;
        public override void Enter()
        {
            inputService.SwitchToGameplay();
            Debug.Log("Entering PlayingState");
        }

        public override void Exit()
        {
        }
    }
}