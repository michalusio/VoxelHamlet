using System;

namespace Assets.Scripts.WorldGen.GenSteps
{
    public class PlaceTrees : IGeneratorStep
    {
        private readonly Random Rng;
        private readonly float treeDensity; // tree density per block squared
        public PlaceTrees(Random rng, float treeDensity)
        {
            Rng = rng;
            this.treeDensity = treeDensity;
        }

        public void Commit(CubeMap map)
        {
            var treeTries = (long)(map.W * map.D * (double)treeDensity);
            for (long i = 0; i < treeTries; i++)
            {
                var x = Rng.Next(0, map.W - 1);
                var z = Rng.Next(0, map.D - 1);
                var y = Rng.Next(0, UnityEngine.Mathf.Min(map.GetHighestYAt(x, z), map.H - 1));
            }
        }
    }
}
