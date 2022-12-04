using Assets.Scripts.Utilities;
using Assets.Scripts.WorldGen;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Assets.Scripts.PathFinding
{
    internal class NavMeshPlane
    {
        public byte minX;
        public byte maxX;
        public byte minZ;
        public byte maxZ;
        public byte Y;

        public HashSet<NavMeshPlane> Neighbours = new HashSet<NavMeshPlane>();

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

            return true;
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
                    plane.Neighbours.Add(this);
                    Neighbours.Add(plane);
                }
            }
        }
    }
}
