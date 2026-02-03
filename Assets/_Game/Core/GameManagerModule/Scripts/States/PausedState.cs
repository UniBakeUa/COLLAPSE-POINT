using System;
using _Game.Core.StateMashineModule.Scripts;
using UnityEngine;

namespace _Game.Core.GameManagerModule.Scripts.States
{
    public class PausedState : StateBase
    {
        public override void Enter() => Debug.Log("Game is PAUSED");
        public override void Exit() => Debug.Log("Exiting PAUSED");
    }
}