namespace _Game.Core.StateMashineModule.Scripts
{
    public abstract class PayLoadedStateBase<TPayload> : StateBase 
    {
        public virtual void Enter(TPayload payLoad)
        {
            Enter();
        }
    }
}