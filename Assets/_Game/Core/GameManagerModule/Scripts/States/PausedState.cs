using System;
using _Game.Core.StateMashineModule.Scripts;

namespace _Game.Core.GameManagerModule.Scripts.States
{
    public class PausedState : StateBase
    {
        public override void Enter() => Console.WriteLine("Game is PAUSED");
        public override void Exit() => Console.WriteLine("Exiting PAUSED");
    }
}