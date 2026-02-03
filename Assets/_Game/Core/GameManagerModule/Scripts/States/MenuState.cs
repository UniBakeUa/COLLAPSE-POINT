using System;
using _Game.Core.StateMashineModule.Scripts;
using UnityEngine;

namespace _Game.Core.GameManagerModule.Scripts.States
{
    public class MenuState : StateBase
    {
        public override void Enter() => Debug.Log("Menu Open");
        public override void Exit() => Debug.Log("Menu Close");
    }
}