using System;
using _Game.Core.StateMashineModule.Scripts;

namespace _Game.Core.GameManagerModule.Scripts.States
{
    public class VictoryState : StateBase
    {
        public override void Enter() => Console.WriteLine("Victory!");
        public override void Exit() => Console.WriteLine("Exiting Victory");
    }
}