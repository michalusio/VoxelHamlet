using LibNoise;
using LibNoise.Generator;
using UnityEngine;

namespace Assets.Scripts.WorldGen.GenSteps
{
    public class PlaceTerrain : IGeneratorStep
    {
        private readonly int MinY;
        private readonly int MaxY;

        public PlaceTerrain(int minY, int maxY)
        {
            MinY = minY;
            MaxY = maxY;
        }

        public void Commit(CubeMap map)
        {
            var noise = new RidgedMultifractal(1, 2, 3, Time.frameCount, QualityMode.High);
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
                        var normalized = (float)noise.GetValue(noiseX / 80f, noiseZ / 80f, 0.5) / 1.875f;
                        var h = MinY + Mathf.RoundToInt((MaxY - MinY) * (normalized + 1) / 4) * 4;

                        if (h > chunkY) kv.Value.Dirty = true;

                        for (int y = chunkY; y < Mathf.Min(chunkY + CubeMap.RegionSize, h); y++)
                        {
                            Block b = default;
                            b.BlockType = h > 52 ? BlockType.Stone : y == h - 1 ? BlockType.Grass : y >= h - 4 ? BlockType.Dirt : BlockType.Stone;

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
