using Assets.Scripts.PathFinding;
using Assets.Scripts.Utilities;
using LibNoise;
using LibNoise.Generator;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.WorldGen.GenSteps
{
    public class ChopBottom : IGeneratorStep
    {
        public void Commit(CubeMap map)
        {
            Parallel.ForEach(map.GetChunks, kv =>
            {
                int chunkY = kv.Key.y * CubeMap.RegionSize;
                Vector3Int bVec = default;
                bVec.x = (map.W >> 3);
                bVec.y = 0;
                bVec.z = (map.D >> 3);

                for (int x = 0; x < CubeMap.RegionSize >> 2; x++)
                {
                    int noiseX = x + kv.Key.x * (CubeMap.RegionSize >> 2);
                    int realX = x << 2;

                    for (int z = 0; z < CubeMap.RegionSize >> 2; z++)
                    {
                        int noiseZ = z + kv.Key.z * (CubeMap.RegionSize >> 2);
                        int realZ = z << 2;

                        Vector3Int aVec = default;
                        aVec.x = noiseX;
                        aVec.y = 0;
                        aVec.z = noiseZ;

                        var h = (aVec - bVec).Abs().Sum() / 4;

                        if (h > chunkY) kv.Value.Dirty = true;

                        for (int y = chunkY; y < Mathf.Min(chunkY + CubeMap.RegionSize, h); y++)
                        {
                            Block b = default;
                            b.BlockType = BlockType.Air;

                            int blockY = y - chunkY;

                            kv.Value[realX, blockY, realZ] = b;
                            kv.Value[realX + 1, blockY, realZ] = b;
                            kv.Value[realX, blockY, realZ + 1] = b;
                            kv.Value[realX + 1, blockY, realZ + 1] = b;

                            kv.Value[realX + 2, blockY, realZ] = b;
                            kv.Value[realX + 3, blockY, realZ] = b;
                            kv.Value[realX + 2, blockY, realZ + 1] = b;
                            kv.Value[realX + 3, blockY, realZ + 1] = b;

                            kv.Value[realX, blockY, realZ + 2] = b;
                            kv.Value[realX + 1, blockY, realZ + 2] = b;
                            kv.Value[realX, blockY, realZ + 3] = b;
                            kv.Value[realX + 1, blockY, realZ + 3] = b;

                            kv.Value[realX + 2, blockY, realZ + 2] = b;
                            kv.Value[realX + 3, blockY, realZ + 2] = b;
                            kv.Value[realX + 2, blockY, realZ + 3] = b;
                            kv.Value[realX + 3, blockY, realZ + 3] = b;
                        }
                    }
                }
            });
        }
    }
}
