using Assets.Scripts.Utilities;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Assets.Scripts.WorldGen
{
    public enum BlockDirection : int
    {
        SELF = 0,

        TOP = 1,
        BOTTOM = -1,
        NORTH = 2,
        SOUTH = -2,
        WEST = 3,
        EAST = -3,

        NegX = WEST,
        NegY = BOTTOM,
        NegZ = SOUTH,
        PosX = EAST,
        PosY = TOP,
        PosZ = NORTH
    }

    public static class BlockDirectionUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3Int GetDisplacement(this BlockDirection dir)
        {
            Vector3Int result = default;
            result.x = result.y = result.z = 0;
            switch (dir)
            {
                case BlockDirection.TOP:
                    result.y = 1;
                    break;
                case BlockDirection.BOTTOM:
                    result.y = -1;
                    break;
                case BlockDirection.NORTH:
                    result.z = 1;
                    break;
                case BlockDirection.SOUTH:
                    result.z = -1;
                    break;
                case BlockDirection.EAST:
                    result.x = 1;
                    break;
                case BlockDirection.WEST:
                    result.x = -1;
                    break;
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3Int GetBlockAtFace(this Vector3Int b, BlockDirection dir)
        {
            var displacement = dir.GetDisplacement();
            Vector3Int newV = default;
            newV.x = b.x + displacement.x;
            newV.y = b.y + displacement.y;
            newV.z = b.z + displacement.z;

            var map = GlobalSettings.Instance.Map;

            if (newV.x < 0 || newV.x >= map.W || newV.y < 0 || newV.y >= map.H || newV.z < 0 || newV.z >= map.D) return b;
            return newV;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BlockDirection GetOppositeFace(this BlockDirection dir)
        {
            return (BlockDirection)(-(int)dir);
        }
    }
}
