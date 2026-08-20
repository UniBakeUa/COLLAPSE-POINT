using System.Collections.Generic;
using _Game.Core.GlobalStateMachine.Scripts.States;
using _Game.Core.StateMachineModule.Scripts;
using Zenject;

namespace _Game.Core.GlobalStateMachine.Scripts
{
    public class GlobalStateMachine : StateMachineBehaviour<GlobalStateBase>, IInitializable
    {
        private readonly SignalBus _signalBus;
        public GlobalStateMachine(List<GlobalStateBase> states, SignalBus signalBus)
        {
            SetStates(states);
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<BootstrapCompletedSignal>(EnterMainMenu);
            Enter<BootstrapState>();
        }

        public void EnterMainMenu()
        {
            Enter<MainMenuState>();
        }
        public void EnterPlaying()
        {
            Enter<GameplayState>();
        }

        public new void Dispose()
        {
            _signalBus.Unsubscribe<BootstrapCompletedSignal>(EnterMainMenu);
            base.Dispose();
        }
    }
}