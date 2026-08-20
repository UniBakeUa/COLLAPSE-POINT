using UnityEngine;
using Zenject;

namespace _Game.Core.GameplayStateMachine.Scripts.States
{
    public class GameplayLoad : GameplayStateBase
    {
        private SignalBus _signalBus;
        public GameplayLoad(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }
        public override void Enter()
        {
            Debug.Log("Entering GameplayLoad");
            _signalBus.Fire<GameplayLoadComplitedSignal>();
        }

        public override void Exit()
        {
            
        }
    }
}