using Assets.Scripts.Entities;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Assets.Scripts.WorldGen
{
    [StructLayout(LayoutKind.Sequential)]
    public struct GreedyCube
    {
        public Entity entity;
        public BlockType id;
        public byte sx, sy, sz;
        public byte ex, ey, ez;
        public byte ORFaceCount;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BlockMesh(bool[] alreadyMeshed)
        {
            for (byte iz = sz; iz <= ez; iz++)
            {
                for (byte iy = sy; iy <= ey; iy++)
                {
                    for (byte ix = sx; ix <= ex; ix++)
                    {
                        var index = iz * CubeMap.RegionSizeSquared + iy * CubeMap.RegionSize + ix;
                        alreadyMeshed[index] = true;
                    }
                }
            }
        }

        public override string ToString()
        {
            return $"<{sx}|{sy}|{sz}> by <{ex}|{ey}|{ez}> ({id})";
        }
    }
}