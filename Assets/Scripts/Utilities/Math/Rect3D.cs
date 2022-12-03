using UnityEngine;

namespace Assets.Scripts.Utilities.Math
{
    public struct Rect3Int
    {
        public Vector3Int Min, Max;

        public Vector3Int Size => Max - Min + Vector3Int.one;

        public Rect3Int(Vector3Int min, Vector3Int max)
        {
            Max = Vector3Int.Max(min, max);
            Min = Vector3Int.Min(min, max);
        }

        public bool Inside(Vector3Int position)
        {
            return position.x >= Min.x && position.y >= Min.y && position.z >= Min.z && position.x <= Max.x && position.y <= Max.y && position.z <= Max.z;
        }

        public bool Intersects(Rect3Int r)
        {
            return (Min.x <= r.Max.x && Max.x >= r.Min.x) &&
                    (Min.y <= r.Max.y && Max.y >= r.Min.y) &&
                    (Min.z <= r.Max.z && Max.z >= r.Min.z);
        }
    }
}
