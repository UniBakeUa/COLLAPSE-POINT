using System;
using _Game.Core.StateMashineModule.Scripts;
using UnityEngine;

namespace _Game.Core.GameManagerModule.Scripts.States
{
    public class GameoverState : StateBase
    {
        public override void Enter() => Debug.Log("Game Over");
        public override void Exit() => Debug.Log("Exiting GameOver");
    }
}