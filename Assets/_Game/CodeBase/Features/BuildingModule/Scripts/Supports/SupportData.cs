using UnityEngine;

namespace _Game.CodeBase.Features.BuildingModule.Scripts.Data
{
    public struct SupportData
    {
        public int Id;
        public Vector2 Start;
        public Vector2 End;
        public int Generation;
        public int ParentRoomId;
        public float Thickness { get; }

        public SupportData(int id, Vector2 start, Vector2 end, int parentRoomId, int generation,float thickness)
        {
            Id = id;
            Start = start;
            End = end;
            ParentRoomId = parentRoomId;
            Generation = generation;
            Thickness = thickness;
        }

        public Vector2 GetPointOnLine(float margin)
        {
            return Vector2.Lerp(Start, End, Random.Range(margin, 1f - margin));
        }
    }
}