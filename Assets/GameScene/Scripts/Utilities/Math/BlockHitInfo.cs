using Assets.Scripts.WorldGen;
using UnityEngine;

namespace Assets.Scripts.Utilities.Math
{
    public static partial class RaycastUtils
    {
        public struct BlockHitInfo
        {
            public readonly Block Block;
            public readonly Vector3Int Position;
            public readonly Vector3 Normal;

            public BlockHitInfo(Block block, Vector3Int position, Vector3 normal)
            {
                Block = block;
                Position = position;
                Normal = normal;
            }
        }
    }
}