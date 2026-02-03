using System;
using _Game.Core.StateMashineModule.Scripts;

namespace _Game.Core.GameManagerModule.Scripts.States
{
    public class PlayingState : StateBase
    {
        public override void Enter() => Console.WriteLine("Game is PLAYING");
        public override void Exit() => Console.WriteLine("Exiting PLAYING");
    }
}