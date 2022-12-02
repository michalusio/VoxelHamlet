using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.WorldGen.GenSteps
{
    public class PlaceTrees : IGeneratorStep
    {
        private readonly float treeDensity; // tree density per block squared
        public PlaceTrees(float treeDensity)
        {
            this.treeDensity = treeDensity;
        }

        private readonly ConcurrentDictionary<int, Random> ThreadRandoms = new ConcurrentDictionary<int, Random>();

        public void Commit(CubeMap map)
        {
            var treeTries = (long)(map.W * map.D * (double)treeDensity);
            Parallel.For(0, treeTries, _ =>
            {
                var random = ThreadRandoms.GetOrAdd(Thread.CurrentThread.ManagedThreadId, _ => new Random(new object().GetHashCode()));
                var x = random.Next(0, map.W - 1);
                var z = random.Next(0, map.D - 1);
                var y = random.Next(0, UnityEngine.Mathf.Min(map.GetHighestYAt(x, z), map.H - 1));
            });
        }
    }
}
