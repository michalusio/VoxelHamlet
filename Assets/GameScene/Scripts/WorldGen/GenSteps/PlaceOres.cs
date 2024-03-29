﻿using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Assets.Scripts.WorldGen.GenSteps
{
    public class PlaceOres : IGeneratorStep
    {
        private readonly Random Rng;
        private readonly float oreDensity; // ore density per block cubed
        private const float GOLD_CHANCE = 0.2f;
        private const float IRON_CHANCE = 0.3f;
        public PlaceOres(Random rng, float oreDensity)
        {
            Rng = rng;
            this.oreDensity = oreDensity;
        }


        public void Commit(CubeMap map)
        {
            var oreTries = (long)((long)map.W * map.H * map.D * (double)oreDensity);
            for (long i = 0; i < oreTries; i++)
            {
                var x = Rng.Next(0, map.W - 1);
                var z = Rng.Next(0, map.D - 1);
                var y = Rng.Next(0, UnityEngine.Mathf.Min(map.GetHighestYAt(x, z), map.H - 1));

                var blockRoll = Rng.NextDouble();
                BlockType chosenBlock = BlockType.CoalOre;
                if (blockRoll < GOLD_CHANCE)
                {
                    chosenBlock = BlockType.GoldOre;
                }
                else if (blockRoll < IRON_CHANCE + GOLD_CHANCE)
                {
                    chosenBlock = BlockType.IronOre;
                }
                PlaceOreBlob(map, chosenBlock, x, y, z);
            }
        }

        private void PlaceOreBlob(CubeMap map, BlockType chosenBlock, int x, int y, int z)
        {
            Block block = default;
            block.BlockType = chosenBlock;
            for (int a = 0; a < 2; a++)
            {
                for (int b = 0; b < 2; b++)
                {
                    for (int c = 0; c < 2; c++)
                    {
                        if (map[x + a, y + c, z + b].BlockType == BlockType.Stone) map[x + a, y + c, z + b] = block;
                    }
                }
            }
        }
    }
}
