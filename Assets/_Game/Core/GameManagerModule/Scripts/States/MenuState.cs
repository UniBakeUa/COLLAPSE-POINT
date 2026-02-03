using System;
using _Game.Core.StateMashineModule.Scripts;

namespace _Game.Core.GameManagerModule.Scripts.States
{
    public class MenuState : StateBase
    {
        public override void Enter() => Console.WriteLine("Menu Open");
        public override void Exit() => Console.WriteLine("Menu Close");
    }
}