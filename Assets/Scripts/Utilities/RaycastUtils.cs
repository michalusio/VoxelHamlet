using System;
using System.Collections.Generic;
using Assets.Scripts.WorldGen;
using UnityEngine;

namespace Assets.Scripts.Utilities
{
    public static partial class RaycastUtils
    {
        internal static bool Raycast(CubeMap map, Ray ray, out BlockHitInfo hitInfo)
        {
            return Raycast(map, ray, b => b.BlockType != BlockType.Air, out hitInfo);
        }
        internal static bool Raycast(CubeMap map, Ray ray, Predicate<Block> condition, out BlockHitInfo hitInfo)
        {
            return Raycast(map, ray, condition, 500, out hitInfo);
        }
        internal static bool Raycast(CubeMap map, Ray ray, int maxDist, out BlockHitInfo hitInfo)
        {
            return Raycast(map, ray, b => b.BlockType != BlockType.Air, maxDist, out hitInfo);
        }
        internal static bool Raycast(CubeMap map, Ray ray, Predicate<Block> condition, int maxDist, out BlockHitInfo hitInfo)
        {
            if (map == null)
            {
                hitInfo = default;
                return false;
            }
            var t = 0.0f;

            Vector3Int ib = Vector3Int.FloorToInt(ray.origin);

            int stepx = (ray.direction.x > 0) ? 1 : -1;
            int stepy = (ray.direction.y > 0) ? 1 : -1;
            int stepz = (ray.direction.z > 0) ? 1 : -1;

            // dx,dy,dz are already normalized
            Vector3 tDelta = ray.direction.Inverse().Abs();

            float xdist = (stepx > 0) ? (ib.x + 1 - ray.origin.x) : (ray.origin.x - ib.x);
            float ydist = (stepy > 0) ? (ib.y + 1 - ray.origin.y) : (ray.origin.y - ib.y);
            float zdist = (stepz > 0) ? (ib.z + 1 - ray.origin.z) : (ray.origin.z - ib.z);

            // location of nearest voxel boundary, in units of t 
            float txMax = (tDelta.x < float.PositiveInfinity) ? tDelta.x * xdist : float.PositiveInfinity;
            float tyMax = (tDelta.y < float.PositiveInfinity) ? tDelta.y * ydist : float.PositiveInfinity;
            float tzMax = (tDelta.z < float.PositiveInfinity) ? tDelta.z * zdist : float.PositiveInfinity;

            int steppedIndex = -1;

            bool wasInMap = false;
            // main loop along raycast vector
            while (t <= maxDist)
            {
                if (map.IsInBounds(ib))
                {
                    wasInMap = true;
                    // exit check
                    var b = map[ib];
                    if (condition(b))
                    {
                        Vector3 normal = Vector3.zero;
                        if (steppedIndex == 0) normal.x = -stepx;
                        if (steppedIndex == 1) normal.y = -stepy;
                        if (steppedIndex == 2) normal.z = -stepz;

                        hitInfo = new BlockHitInfo(b, ib, normal);
                        return true;
                    }
                }
                else
                {
                    if (wasInMap) break;
                }

                // advance t to next nearest voxel boundary
                if (txMax < tyMax)
                {
                    if (txMax < tzMax)
                    {
                        ib.x += stepx;
                        t = txMax;
                        txMax += tDelta.x;
                        steppedIndex = 0;
                    }
                    else
                    {
                        ib.z += stepz;
                        t = tzMax;
                        tzMax += tDelta.z;
                        steppedIndex = 2;
                    }
                }
                else
                {
                    if (tyMax < tzMax)
                    {
                        ib.y += stepy;
                        t = tyMax;
                        tyMax += tDelta.y;
                        steppedIndex = 1;
                    }
                    else
                    {
                        ib.z += stepz;
                        t = tzMax;
                        tzMax += tDelta.z;
                        steppedIndex = 2;
                    }
                }

            }

            hitInfo = default;
            return false;
        }

        internal static bool RaycastOOB(CubeMap map, Ray ray, out BlockHitInfo hitInfo)
        {
            return RaycastOOB(map, ray, b => b.BlockType != BlockType.Air, out hitInfo);
        }
        internal static bool RaycastOOB(CubeMap map, Ray ray, Predicate<Block> condition, out BlockHitInfo hitInfo)
        {
            return RaycastOOB(map, ray, condition, 500, out hitInfo);
        }
        internal static bool RaycastOOB(CubeMap map, Ray ray, int maxDist, out BlockHitInfo hitInfo)
        {
            return RaycastOOB(map, ray, b => b.BlockType != BlockType.Air, maxDist, out hitInfo);
        }
        internal static bool RaycastOOB(CubeMap map, Ray ray, Predicate<Block> condition, int maxDist, out BlockHitInfo hitInfo)
        {
            if (map == null)
            {
                hitInfo = default;
                return false;
            }
            var t = 0.0f;

            Vector3Int ib = Vector3Int.FloorToInt(ray.origin);

            int stepx = (ray.direction.x > 0) ? 1 : -1;
            int stepy = (ray.direction.y > 0) ? 1 : -1;
            int stepz = (ray.direction.z > 0) ? 1 : -1;

            // dx,dy,dz are already normalized
            Vector3 tDelta = ray.direction.Inverse().Abs();

            float xdist = (stepx > 0) ? (ib.x + 1 - ray.origin.x) : (ray.origin.x - ib.x);
            float ydist = (stepy > 0) ? (ib.y + 1 - ray.origin.y) : (ray.origin.y - ib.y);
            float zdist = (stepz > 0) ? (ib.z + 1 - ray.origin.z) : (ray.origin.z - ib.z);

            // location of nearest voxel boundary, in units of t 
            float txMax = (tDelta.x < float.PositiveInfinity) ? tDelta.x * xdist : float.PositiveInfinity;
            float tyMax = (tDelta.y < float.PositiveInfinity) ? tDelta.y * ydist : float.PositiveInfinity;
            float tzMax = (tDelta.z < float.PositiveInfinity) ? tDelta.z * zdist : float.PositiveInfinity;

            int steppedIndex = -1;

            bool wasInMap = false;
            // main loop along raycast vector
            while (t <= maxDist)
            {
                if (map.IsInBounds(ib))
                {
                    wasInMap = true;
                    // exit check
                    var b = map[ib];
                    if (condition(b))
                    {
                        Vector3 normal = Vector3.zero;
                        if (steppedIndex == 0) normal.x = -stepx;
                        if (steppedIndex == 1) normal.y = -stepy;
                        if (steppedIndex == 2) normal.z = -stepz;

                        hitInfo = new BlockHitInfo(b, ib, normal);
                        return true;
                    }
                }
                else
                {
                    if (wasInMap)
                    {
                        Vector3 normal = Vector3.zero;
                        if (steppedIndex == 0) normal.x = -stepx;
                        if (steppedIndex == 1) normal.y = -stepy;
                        if (steppedIndex == 2) normal.z = -stepz;

                        hitInfo = new BlockHitInfo(default, ib, normal);
                        return true;
                    }
                }

                // advance t to next nearest voxel boundary
                if (txMax < tyMax)
                {
                    if (txMax < tzMax)
                    {
                        ib.x += stepx;
                        t = txMax;
                        txMax += tDelta.x;
                        steppedIndex = 0;
                    }
                    else
                    {
                        ib.z += stepz;
                        t = tzMax;
                        tzMax += tDelta.z;
                        steppedIndex = 2;
                    }
                }
                else
                {
                    if (tyMax < tzMax)
                    {
                        ib.y += stepy;
                        t = tyMax;
                        tyMax += tDelta.y;
                        steppedIndex = 1;
                    }
                    else
                    {
                        ib.z += stepz;
                        t = tzMax;
                        tzMax += tDelta.z;
                        steppedIndex = 2;
                    }
                }

            }

            hitInfo = default;
            return false;
        }

        internal static List<BlockHitInfo> RaycastList(CubeMap map, Ray ray, int maxDist)
        {
            var result = new List<BlockHitInfo>();
            if (map == null)
            {
                return result;
            }

            var t = 0.0f;

            Vector3Int ib = Vector3Int.FloorToInt(ray.origin);

            int stepx = (ray.direction.x > 0) ? 1 : -1;
            int stepy = (ray.direction.y > 0) ? 1 : -1;
            int stepz = (ray.direction.z > 0) ? 1 : -1;

            // dx,dy,dz are already normalized
            Vector3 tDelta = ray.direction.Inverse().Abs();

            float xdist = (stepx > 0) ? (ib.x + 1 - ray.origin.x) : (ray.origin.x - ib.x);
            float ydist = (stepy > 0) ? (ib.y + 1 - ray.origin.y) : (ray.origin.y - ib.y);
            float zdist = (stepz > 0) ? (ib.z + 1 - ray.origin.z) : (ray.origin.z - ib.z);

            // location of nearest voxel boundary, in units of t 
            float txMax = (tDelta.x < float.PositiveInfinity) ? tDelta.x * xdist : float.PositiveInfinity;
            float tyMax = (tDelta.y < float.PositiveInfinity) ? tDelta.y * ydist : float.PositiveInfinity;
            float tzMax = (tDelta.z < float.PositiveInfinity) ? tDelta.z * zdist : float.PositiveInfinity;

            int steppedIndex = -1;

            bool wasInMap = false;
            // main loop along raycast vector
            while (t <= maxDist)
            {
                if (map.IsInBounds(ib))
                {
                    wasInMap = true;
                    // exit check
                    var b = map[ib];
                    {
                        Vector3 normal = Vector3.zero;
                        if (steppedIndex == 0) normal.x = -stepx;
                        if (steppedIndex == 1) normal.y = -stepy;
                        if (steppedIndex == 2) normal.z = -stepz;

                        result.Add(new BlockHitInfo(b, ib, normal));
                    }
                }
                else
                {
                    if (wasInMap) break;
                }

                // advance t to next nearest voxel boundary
                if (txMax < tyMax)
                {
                    if (txMax < tzMax)
                    {
                        ib.x += stepx;
                        t = txMax;
                        txMax += tDelta.x;
                        steppedIndex = 0;
                    }
                    else
                    {
                        ib.z += stepz;
                        t = tzMax;
                        tzMax += tDelta.z;
                        steppedIndex = 2;
                    }
                }
                else
                {
                    if (tyMax < tzMax)
                    {
                        ib.y += stepy;
                        t = tyMax;
                        tyMax += tDelta.y;
                        steppedIndex = 1;
                    }
                    else
                    {
                        ib.z += stepz;
                        t = tzMax;
                        tzMax += tDelta.z;
                        steppedIndex = 2;
                    }
                }

            }

            return result;
        }
    }
}