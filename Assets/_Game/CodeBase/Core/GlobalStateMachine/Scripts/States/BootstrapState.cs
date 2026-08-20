using UnityEngine;
using Zenject;

namespace _Game.Core.GlobalStateMachine.Scripts.States
{
    public class BootstrapState : GlobalStateBase
    {
        private readonly SignalBus _signalBus;

        public BootstrapState(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }
        public override void Enter()
        {
            Debug.Log("Bootstrap State Enter");
            
            _signalBus.Fire<BootstrapCompletedSignal>();
        }

        public override void Exit() { }
    }
}