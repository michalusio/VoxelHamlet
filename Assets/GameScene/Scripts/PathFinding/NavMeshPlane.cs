using Assets.Scripts.Utilities;
using Assets.Scripts.WorldGen;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Assets.Scripts.PathFinding
{
    public class NavMeshPlane
    {
        public byte minX;
        public byte maxX;
        public byte minZ;
        public byte maxZ;
        public byte Y;
        public readonly NavMeshChunk Chunk;

        public readonly HashSet<NavMeshPlane> Neighbours = new HashSet<NavMeshPlane>();

        public NavMeshPlane(NavMeshChunk chunk)
        {
            Chunk = chunk;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BlockMesh(bool[] alreadyMeshed)
        {
            for (var iz = minZ; iz <= maxZ; iz++)
            {
                for (var ix = minX; ix <= maxX; ix++)
                {
                    var alreadyIndex = (((iz << CubeMap.RegionSizeShift) + Y) << CubeMap.RegionSizeShift) + ix;
                    alreadyMeshed[alreadyIndex] = true;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnblockMesh(bool[] alreadyMeshed)
        {
            for (var iz = minZ; iz <= maxZ; iz++)
            {
                for (var ix = minX; ix <= maxX; ix++)
                {
                    var alreadyIndex = (((iz << CubeMap.RegionSizeShift) + Y) << CubeMap.RegionSizeShift) + ix;
                    alreadyMeshed[alreadyIndex] = false;
                }
            }
        }

        public void RemoveConnections()
        {
            foreach(var plan in Neighbours)
            {
                plan.Neighbours.Remove(this);
            }
            Neighbours.Clear();
        }

        public bool Touches(NavMeshPlane other)
        {
            // has horizontal gap
            if (minX > other.maxX + 1 || other.minX > maxX + 1) return false;

            // has vertical gap
            if (minZ > other.maxZ + 1 || other.minZ > maxZ + 1) return false;

            // planes touch only if they have more than one meter of shared edge
            //if (Mathf.Min(maxX, other.maxX) - Mathf.Max(minX, other.minX) < 1 && Mathf.Min(maxZ, other.maxZ) - Mathf.Max(minZ, other.minZ) < 1) return false;
            // Disabled it for now as there could be problems in passageways in the direction perpendicular to the meshing direction

            return this != other;
        }

        public int Area()
        {
            return (maxX - minX + 1) * (maxZ - minZ + 1);
        }

        public override string ToString()
        {
            return $"({minX}, {minZ}) to ({maxX}, {maxZ}) [Y: {Y}]";
        }

        public void AddConnections(NavMeshChunk navChunk)
        {
            var watch = Stopwatch.StartNew();
            ConnectPlane(navChunk, Y);
            ConnectPlane(navChunk, Y - 1);
            ConnectPlane(navChunk, Y + 1);
            if (minX == 0 && navChunk.ChunkNX != null)
            {
                var planes = navChunk.ChunkNX.NavMeshPlanes;
                if (planes.ContainsKey(Y))
                {
                    foreach (var plane in planes[Y])
                    {
                        if (plane.maxX == CubeMap.RegionSize - 1)
                        {
                            if (minZ > plane.maxZ + 1 || plane.minZ > maxZ + 1) continue;
                            Connect(plane);
                        }
                    }
                }
                if (planes.ContainsKey(Y - 1))
                {
                    foreach (var plane in planes[Y - 1])
                    {
                        if (plane.maxX == CubeMap.RegionSize - 1)
                        {
                            if (minZ > plane.maxZ + 1 || plane.minZ > maxZ + 1) continue;
                            Connect(plane);
                        }
                    }
                }
                if (planes.ContainsKey(Y + 1))
                {
                    foreach (var plane in planes[Y + 1])
                    {
                        if (plane.maxX == CubeMap.RegionSize - 1)
                        {
                            if (minZ > plane.maxZ + 1 || plane.minZ > maxZ + 1) continue;
                            Connect(plane);
                        }
                    }
                }
            }
            if (minZ == 0 && navChunk.ChunkNZ != null)
            {
                var planes = navChunk.ChunkNZ.NavMeshPlanes;
                if (planes.ContainsKey(Y))
                {
                    foreach (var plane in planes[Y])
                    {
                        if (plane.maxZ == CubeMap.RegionSize - 1)
                        {
                            if (minX > plane.maxX + 1 || plane.minX > maxX + 1) continue;
                            Connect(plane);
                        }
                    }
                }
                if (planes.ContainsKey(Y - 1))
                {
                    foreach (var plane in planes[Y - 1])
                    {
                        if (plane.maxZ == CubeMap.RegionSize - 1)
                        {
                            if (minX > plane.maxX + 1 || plane.minX > maxX + 1) continue;
                            Connect(plane);
                        }
                    }
                }
                if (planes.ContainsKey(Y + 1))
                {
                    foreach (var plane in planes[Y + 1])
                    {
                        if (plane.maxZ == CubeMap.RegionSize - 1)
                        {
                            if (minX > plane.maxX + 1 || plane.minX > maxX + 1) continue;
                            Connect(plane);
                        }
                    }
                }
            }
            watch.Stop();
            DynamicLogger.Log("Touch", $"Connected plane in {watch.ElapsedMilliseconds}ms");
        }

        private void ConnectPlane(NavMeshChunk navChunk, int y)
        {
            if (!navChunk.NavMeshPlanes.ContainsKey(y)) return;
            foreach (var plane in navChunk.NavMeshPlanes[y])
            {
                if (Touches(plane))
                {
                    Connect(plane);
                }
            }
        }

        private void Connect(NavMeshPlane plane)
        {
            plane.Neighbours.Add(this);
            Neighbours.Add(plane);
        }
    }
}
