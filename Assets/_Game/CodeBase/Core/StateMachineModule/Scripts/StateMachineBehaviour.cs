using System;
using System.Collections.Generic;

namespace _Game.Core.StateMachineModule.Scripts
{
    public class StateMachineBehaviour<TStateBase> : IDisposable 
        where TStateBase : StateBase
    {
        protected TStateBase ActiveStateBase;
        protected Dictionary<Type, TStateBase> States;
        
        public virtual Type GetCurrentStateType() =>
            ActiveStateBase?.GetType();
        
        public TStateBase GetCurrentStateInstance() => ActiveStateBase;
        protected void SetStates(List<TStateBase> states) 
        {
            States = new ();
            foreach (var state in states)
                States.Add(state.GetType(), state);
        }

        protected void Enter<TState>() where TState : TStateBase 
        {
            ActiveStateBase?.Exit();
            ActiveStateBase = States[typeof(TState)];
            ActiveStateBase.Enter();
        }
        
        protected virtual void Enter<TState, TPayload>(TPayload payLoad)
            where TState : PayLoadedStateBase<TPayload>
        {
            ActiveStateBase.Exit();
            
            var newState = States[typeof(TState)] as TState;
            newState?.Enter(payLoad);
            
            ActiveStateBase = newState as TStateBase;
        }
        public void Dispose() 
        {
            ActiveStateBase?.Exit();
            States.Clear();
            ActiveStateBase = null;
        }
    }
}