using System;
using UnityEngine;

namespace _Game.CodeBase.Features.BuildingModule.Scripts.Data
{
    [Serializable]
    public class SupportMaterialLevel
    {
        public string name;
        public float maxLoad;
        public float buildTime;
        public Sprite supportSprite;
        public Color visualColor = Color.white;
        public GameObject supportPrefab;
    }
}