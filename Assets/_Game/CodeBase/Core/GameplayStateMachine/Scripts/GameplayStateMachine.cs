using System.Collections.Generic;
using _Game.Core.GameplayStateMachine.Scripts.States;
using _Game.Core.GlobalStateMachine.Scripts;
using _Game.Core.StateMachineModule.Scripts;
using Zenject;

namespace _Game.Core.GameplayStateMachine.Scripts
{
    public class GameplayStateMachine : StateMachineBehaviour<GameplayStateBase>, IInitializable
    {
        private SignalBus _signalBus;

        public GameplayStateMachine(List<GameplayStateBase> states, SignalBus signalBus)
        {
            SetStates(states);
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<GameplayLoadComplitedSignal>(EnterPlaying);
            Enter<GameplayLoad>();
        }

        private void EnterPlaying()
        {
            Enter<PlayingState>();
        }

        public new void Dispose()
        {
            _signalBus.Unsubscribe<BootstrapCompletedSignal>(EnterPlaying);
            base.Dispose();
        }
    }
}