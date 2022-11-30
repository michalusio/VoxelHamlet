using Assets.Scripts.WorldGen;
using System.Collections.Generic;
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
            for (byte iz = minZ; iz <= maxZ; iz++)
            {
                for (byte ix = minX; ix <= maxX; ix++)
                {
                    var index = (iz << CubeMap.RegionSizeShift) + ix;
                    alreadyMeshed[index] = true;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnblockMesh(bool[] alreadyMeshed)
        {
            for (byte iz = minZ; iz <= maxZ; iz++)
            {
                for (byte ix = minX; ix <= maxX; ix++)
                {
                    var index = (iz << CubeMap.RegionSizeShift) + ix;
                    alreadyMeshed[index] = false;
                }
            }
        }

        public void RemoveConnections()
        {
            foreach(var plan in Neighbours)
            {
                plan.Neighbours.Remove(this);
            }
        }

        public int Area()
        {
            return (maxX - minX + 1) * (maxZ - minZ + 1);
        }
    }
}
