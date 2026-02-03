using System;
using _Game.Core.StateMashineModule.Scripts;
using UnityEngine;

namespace _Game.Core.GameManagerModule.Scripts.States
{
    public class LoadingState : StateBase
    {
        public override void Enter() => Debug.Log("Loading...");
        public override void Exit() => Debug.Log("Finished Loading");
    }
}