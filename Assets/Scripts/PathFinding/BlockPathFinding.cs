using Assets.Scripts.ConfigScripts;
using Assets.Scripts.Utilities;
using Assets.Scripts.WorldGen;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Assets.Scripts.PathFinding
{
    public static class BlockPathFinding
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3Int GetFloor(Vector3Int b) => b.GetBlockAtFace(BlockDirection.BOTTOM);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWalkable(this Block b)
        {
            return b.BlockType == BlockType.Air;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWalkable(Vector3Int b)
        {
            return GlobalSettings.Instance.Map[b].IsWalkable();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFloor(Vector3Int b)
        {
            return !GlobalSettings.Instance.Map[GetFloor(b)].IsWalkable();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWalkableAndFloor(Vector3Int b)
        {
            return IsWalkable(b) && HasFloor(b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NodeEqual(Vector3Int a, Vector3Int b) => a == b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Heuristic(Vector3Int a, Vector3Int b) => Distance(a, b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(Vector3Int a, Vector3Int b)
        {
            var dx = Mathf.Abs(a.x - b.x);
            var dy = Mathf.Abs(a.y - b.y) * 1.25f;
            var dz = Mathf.Abs(a.z - b.z);

            const float PARAM = 0.57216878364f;
            const float PARAM2 = 0.57735026919f;

            return PARAM * Mathf.Min((dx + dy + dz) * PARAM2, Mathf.Max(dx, Mathf.Max(dy, dz)));
        }

        internal static int GetFloorCount(Vector3Int b)
        {
            var b00 = b;
            var b10 = b.GetBlockAtFace(BlockDirection.PosX);
            var b01 = b.GetBlockAtFace(BlockDirection.PosZ);
            var b11 = b10.GetBlockAtFace(BlockDirection.PosZ);
            return (
                    (HasFloor(b00) ? 1 : 0) +
                    (HasFloor(b10) ? 1 : 0) +
                    (HasFloor(b01) ? 1 : 0) +
                    (HasFloor(b11) ? 1 : 0)
                );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidForEntity(Vector3Int b)
        {
            var b00 = b;
            var b10 = b;
            b10.x++;
            var b01 = b;
            b01.z++;
            var b11 = b10;
            b11.z++;
            var bt00 = b00;
            bt00.y++;
            var bt10 = b10;
            bt10.y++;
            var bt01 = b01;
            bt01.y++;
            var bt11 = b11;
            bt11.y++;
            var btt00 = bt00;
            btt00.y++;
            var btt10 = bt10;
            btt10.y++;
            var btt01 = bt01;
            btt01.y++;
            var btt11 = bt11;
            btt11.y++;
            return (
                    (HasFloor(b00) ? 1 : 0) +
                    (HasFloor(b10) ? 1 : 0) +
                    (HasFloor(b01) ? 1 : 0) +
                    (HasFloor(b11) ? 1 : 0)
                ) >= 2 && (
                    IsWalkable(b00) &&
                    IsWalkable(b10) &&
                    IsWalkable(b01) &&
                    IsWalkable(b11)
                ) && (
                    IsWalkable(bt00) &&
                    IsWalkable(bt10) &&
                    IsWalkable(bt01) &&
                    IsWalkable(bt11)
                ) && (
                    IsWalkable(btt00) &&
                    IsWalkable(btt10) &&
                    IsWalkable(btt01) &&
                    IsWalkable(btt11)
                );
        }

        public static bool IsFreeForEntity(Vector3Int b)
        {
            var b00 = b;
            var b10 = b;
            b10.x++;
            var b01 = b;
            b01.z++;
            var b11 = b10;
            b11.z++;
            var bt00 = b00;
            bt00.y++;
            var bt10 = b10;
            bt10.y++;
            var bt01 = b01;
            bt01.y++;
            var bt11 = b11;
            bt11.y++;
            var btt00 = bt00;
            btt00.y++;
            var btt10 = bt10;
            btt10.y++;
            var btt01 = bt01;
            btt01.y++;
            var btt11 = bt11;
            btt11.y++;
            return  (
                    IsWalkable(b00) &&
                    IsWalkable(b10) &&
                    IsWalkable(b01) &&
                    IsWalkable(b11)
                ) && (
                    IsWalkable(bt00) &&
                    IsWalkable(bt10) &&
                    IsWalkable(bt01) &&
                    IsWalkable(bt11)
                ) && (
                    IsWalkable(btt00) &&
                    IsWalkable(btt10) &&
                    IsWalkable(btt01) &&
                    IsWalkable(btt11)
                );
        }

        public static List<Vector3Int> GetNeighbours(Vector3Int b, List<Vector3Int> result, bool laddering)
        {
            var map = GlobalSettings.Instance.Map;
            var standing = GetFloorCount(b) != 0;

            if (AddMoves(result, ref b, map) && laddering)
            {
                if (b.y < map.H - 3)
                {
                    var b2 = b;
                    b2.y++;
                    if (IsFreeForEntity(b2))
                    {
                        result.Add(b2);
                    }
                }
                if (b.y > 0)
                {
                    var b2 = b;
                    b2.y--;
                    if (IsFreeForEntity(b2))
                    {
                        result.Add(b2);
                    }
                }
            }

            if (standing)
            {
                if (b.y < map.H - 1)
                {
                    var b2 = b;
                    b2.y++;
                    AddMoves(result, ref b2, map);
                }
                if (b.y > 0)
                {
                    var b2 = b;
                    b2.y--;
                    AddMoves(result, ref b2, map, true);
                }
            }
            
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool AddMoves(List<Vector3Int> result, ref Vector3Int b, CubeMap map, bool noFloor = false)
        {
            bool anyFailed = false;
            var bzn = b.z > 0;
            var bzp = b.z < map.D - 1;
            var bxn = b.x > 0;
            var bxp = b.x < map.W - 1;
            if (bzn)
            {
                var b2 = b;
                b2.z--;
                if ((noFloor && IsFreeForEntity(b2)) || IsValidForEntity(b2))
                {
                    result.Add(b2);
                }
                else anyFailed = true;
            }
            if (bxn)
            {
                var b2 = b;
                b2.x--;
                if ((noFloor && IsFreeForEntity(b2)) || IsValidForEntity(b2))
                {
                    result.Add(b2);
                }
                else anyFailed = true;
            }
            if (bzp)
            {
                var b2 = b;
                b2.z++;
                if ((noFloor && IsFreeForEntity(b2)) || IsValidForEntity(b2))
                {
                    result.Add(b2);
                }
                else anyFailed = true;
            }
            if (bxp)
            {
                var b2 = b;
                b2.x++;
                if ((noFloor && IsFreeForEntity(b2)) || IsValidForEntity(b2))
                {
                    result.Add(b2);
                }
                else anyFailed = true;
            }
            if (bxp && bzn)
            {
                var b2 = b;
                b2.x++;
                b2.z--;
                if ((noFloor && IsFreeForEntity(b2)) || IsValidForEntity(b2))
                {
                    result.Add(b2);
                }
            }
            if (bxn && bzp)
            {
                var b2 = b;
                b2.x--;
                b2.z++;
                if ((noFloor && IsFreeForEntity(b2)) || IsValidForEntity(b2))
                {
                    result.Add(b2);
                }
            }
            if (bxn && bzn)
            {
                var b2 = b;
                b2.x--;
                b2.z--;
                if ((noFloor && IsFreeForEntity(b2)) || IsValidForEntity(b2))
                {
                    result.Add(b2);
                }
            }
            if (bxp && bzp)
            {
                var b2 = b;
                b2.x++;
                b2.z++;
                if ((noFloor && IsFreeForEntity(b2)) || IsValidForEntity(b2))
                {
                    result.Add(b2);
                }
            }
            return anyFailed;
        }
    }
}
