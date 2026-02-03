using System;
using _Game.Core.StateMashineModule.Scripts;
using UnityEngine;

namespace _Game.Core.GameManagerModule.Scripts.States
{
    public class PlayingState : StateBase
    {
        public override void Enter() => Debug.Log("Game is PLAYING");
        public override void Exit() => Debug.Log("Exiting PLAYING");
    }
}