using UnityEngine;

namespace _Game.Content.Features.BuildingModule.Scripts.Supports
{
    public struct SupportData
    {
        public int Id;
        public Vector2 Start;
        public Vector2 End;
        public int Generation;
        public int ParentRoomId;

        public SupportData(int id, Vector2 s, Vector2 e, int rId, int generation)
        {
            Id = id;
            Start = s;
            End = e;
            ParentRoomId = rId;
            Generation = generation;
        }

        public Vector2 GetPointOnLine(float margin)
        {
            return Vector2.Lerp(Start, End, Random.Range(margin, 1f - margin));
        }
    }
}