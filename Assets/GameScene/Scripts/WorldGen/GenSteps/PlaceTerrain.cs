using Assets.General;
using UnityEngine;

namespace Assets.Scripts.WorldGen.GenSteps
{
    public class PlaceTerrain : IGeneratorStep
    {
        private readonly BaseTerrainGenerator Generator;

        public PlaceTerrain(int seed, int MountainModifier, int ForestModifier, int minY, int maxY)
        {
            Generator = new BaseTerrainGenerator(seed, minY, maxY, CrossSceneData.Offset, MountainModifier, ForestModifier);
        }

        public void Commit(CubeMap map)
        {
            foreach (var kv in map.GetChunks)
            {
                int chunkY = kv.Key.y * CubeMap.RegionSize;

                for (int x = 0; x < CubeMap.RegionSize >> 2; x++)
                {
                    int noiseX = x + kv.Key.x * (CubeMap.RegionSize >> 2);
                    int realX = x << 2;
                    for (int z = 0; z < CubeMap.RegionSize >> 2; z++)
                    {
                        int noiseZ = z + kv.Key.z * (CubeMap.RegionSize >> 2);
                        int realZ = z << 2;
                        var h = Generator.GetHeightAt(noiseX * 0.25f, noiseZ * 0.25f);

                        if (h > chunkY) kv.Value.Dirty = true;

                        for (int y = chunkY; y < Mathf.Min(chunkY + CubeMap.RegionSize, h); y++)
                        {
                            Block b = default;
                            b.BlockType = Generator.GetBlockAt(noiseX * 0.25f, y, noiseZ * 0.25f);

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
            }
        }
    }
}
