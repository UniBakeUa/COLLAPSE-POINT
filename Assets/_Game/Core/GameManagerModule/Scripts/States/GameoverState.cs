using System;
using _Game.Core.StateMashineModule.Scripts;

namespace _Game.Core.GameManagerModule.Scripts.States
{
    public class GameoverState : StateBase
    {
        public override void Enter() => Console.WriteLine("Game Over");
        public override void Exit() => Console.WriteLine("Exiting GameOver");
    }
}