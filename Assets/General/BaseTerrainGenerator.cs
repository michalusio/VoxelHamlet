using Assets.Scripts.WorldGen;
using LibNoise;
using LibNoise.Generator;
using LibNoise.Operator;
using UnityEngine;
using Cache = LibNoise.Operator.Cache;

namespace Assets.General
{
    internal class BaseTerrainGenerator
    {
        private readonly ModuleBase HeightNoise;
        private readonly ModuleBase TypeNoise;
        private readonly int MinY;
        private readonly int MaxY;
        private readonly Vector2Int Offset;
        private readonly int MountainModifier;
        private readonly int ForestModifier;

        public BaseTerrainGenerator(int seed, int minY, int maxY, Vector2Int offset, int mountainModifier, int forestModifier)
        {
            MountainModifier = mountainModifier;
            ForestModifier = forestModifier;
            var mountains = new Exponent(2, new Perlin(1, 1, 1, 1, seed + 2, QualityMode.High));
            var heightNoise = new Multiply(new Const(1 / 3.75), new Add(
                new Perlin(1, 2, 0.5, 3, seed, QualityMode.High),
                new Perlin(1, 2, 0.5, 3, seed + 1, QualityMode.High)
            ));
            var complexNoise = new Multiply(new Const(0.5), new Add(
                mountains,
                heightNoise
            ));
            HeightNoise = new Cache(complexNoise);
            TypeNoise = new Turbulence(0.25, new Voronoi(0.75, 1, seed, false));
            MinY = minY;
            MaxY = maxY;
            Offset = offset;
        }
        
        public int GetHeightAt(double x, double z)
        {
            var noiseValue = (float)HeightNoise.GetValue((x + Offset.x) / 10f, 0.5, (z + Offset.y) / 10f);
            var normalized = (noiseValue + 1) / 2;
            return MinY + Mathf.RoundToInt((MaxY - MinY) * normalized / 4) * 4;
        }

        public BlockType GetBlockAt(double x, int y, double z)
        {
            var columnHeight = GetHeightAt(x, z);
            if (columnHeight > MaxY * (0.75f - MountainModifier * 0.05f)) return BlockType.Stone;
            if (y == columnHeight - 1)
            {
                if (TypeNoise.GetValue((x + Offset.x) / 10f, 0.5, (z + Offset.y) / 10f) < (ForestModifier - 1) * 0.75f) return BlockType.ForestGrass;
                else return BlockType.Grass;
            }
            if (y >= columnHeight - 4) return BlockType.Dirt;
            return BlockType.Stone;
        }
    }
}
