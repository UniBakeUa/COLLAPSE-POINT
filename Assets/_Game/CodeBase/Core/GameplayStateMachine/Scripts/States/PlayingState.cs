using UnityEngine;

namespace _Game.Core.GameplayStateMachine.Scripts.States
{
    public class PlayingState : GameplayStateBase
    {
        public override void Enter()
        {
            Debug.Log("Entering PlayingState");
        }

        public override void Exit()
        {
        }
    }
}