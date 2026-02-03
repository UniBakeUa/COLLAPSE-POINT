using System;
using _Game.Core.StateMashineModule.Scripts;
using UnityEngine;

namespace _Game.Core.GameManagerModule.Scripts.States
{
    public class VictoryState : StateBase
    {
        public override void Enter() => Debug.Log("Victory!");
        public override void Exit() => Debug.Log("Exiting Victory");
    }
}