using Assets.Scripts.Entities;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Assets.Scripts.WorldGen
{
    [StructLayout(LayoutKind.Sequential)]
    public struct GreedyCube
    {
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
                        var index = (((iz << CubeMap.RegionSizeShift) + iy) << CubeMap.RegionSizeShift) + ix;
                        alreadyMeshed[index] = true;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Place(Block[] blocks)
        {
            var block = new Block
            {
                BlockType = id
            };
            for (byte iz = sz; iz <= ez; iz++)
            {
                for (byte iy = sy; iy <= ey; iy++)
                {
                    for (byte ix = sx; ix <= ex; ix++)
                    {
                        var index = (((iz << CubeMap.RegionSizeShift) + iy) << CubeMap.RegionSizeShift) + ix;
                        blocks[index] = block;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ToNormalBlock(List<ChunkVertex> vertexList, List<int> indexList, bool isBottomChunk)
        {
            var count = vertexList.Count;

            ChunkVertex v000 = default;
            ChunkVertex v001 = default;
            ChunkVertex v010 = default;
            ChunkVertex v011 = default;
            ChunkVertex v100 = default;
            ChunkVertex v101 = default;
            ChunkVertex v110 = default;
            ChunkVertex v111 = default;

            v000.x = v001.x = v010.x = v011.x = sx;
            v000.y = v001.y = v100.y = v101.y = sy;
            v000.z = v010.z = v100.z = v110.z = sz;

            v100.x = v101.x = v110.x = v111.x = ex;
            v010.y = v011.y = v110.y = v111.y = ey;
            v001.z = v011.z = v101.z = v111.z = ez;

            v000.blockIndex = v001.blockIndex = v010.blockIndex = v011.blockIndex =
            v100.blockIndex = v101.blockIndex = v110.blockIndex = v111.blockIndex = (byte)id;
            if ((ORFaceCount & 8) > 0)
            {
                vertexList.Add(v000);
                vertexList.Add(v010);
                vertexList.Add(v001);
                vertexList.Add(v011);
                indexList.Add(count + 2);
                indexList.Add(count + 3);
                indexList.Add(count + 1);
                indexList.Add(count);
                count += 4;
            }
            if ((ORFaceCount & 16) > 0 && !(isBottomChunk && sy == 0))
            {
                vertexList.Add(v000);
                vertexList.Add(v100);
                vertexList.Add(v001);
                vertexList.Add(v101);
                indexList.Add(count);
                indexList.Add(count + 1);
                indexList.Add(count + 3);
                indexList.Add(count + 2);
                count += 4;
            }
            if ((ORFaceCount & 32) > 0)
            {
                vertexList.Add(v000);
                vertexList.Add(v100);
                vertexList.Add(v010);
                vertexList.Add(v110);
                indexList.Add(count + 2);
                indexList.Add(count + 3);
                indexList.Add(count + 1);
                indexList.Add(count);
                count += 4;
            }
            if ((ORFaceCount & 1) > 0)
            {
                vertexList.Add(v100);
                vertexList.Add(v110);
                vertexList.Add(v101);
                vertexList.Add(v111);
                indexList.Add(count);
                indexList.Add(count + 1);
                indexList.Add(count + 3);
                indexList.Add(count + 2);
                count += 4;
            }
            if ((ORFaceCount & 2) > 0)
            {
                vertexList.Add(v010);
                vertexList.Add(v110);
                vertexList.Add(v011);
                vertexList.Add(v111);
                indexList.Add(count + 2);
                indexList.Add(count + 3);
                indexList.Add(count + 1);
                indexList.Add(count);
                count += 4;
            }
            if ((ORFaceCount & 4) > 0)
            {
                vertexList.Add(v001);
                vertexList.Add(v101);
                vertexList.Add(v011);
                vertexList.Add(v111);
                indexList.Add(count);
                indexList.Add(count + 1);
                indexList.Add(count + 3);
                indexList.Add(count + 2);
            }
        }

        public override string ToString()
        {
            return $"<{sx}|{sy}|{sz}> by <{ex}|{ey}|{ez}> ({id})";
        }
    }
}