using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Game.CodeBase.Features.BuildingModule.Scripts.Data
{
    [Serializable]
    public class RoomData
    {
        public int Id;
        public Vector2 Size;
        public Vector3 Position;
        public float Weight;

        [NonSerialized] public List<int> AttachedSupportIds = new();
    }
}