using System;
using _Game.Core.StateMashineModule.Scripts;

namespace _Game.Core.GameManagerModule.Scripts.States
{
    public class LoadingState : StateBase
    {
        public override void Enter() => Console.WriteLine("Loading...");
        public override void Exit() => Console.WriteLine("Finished Loading");
    }
}