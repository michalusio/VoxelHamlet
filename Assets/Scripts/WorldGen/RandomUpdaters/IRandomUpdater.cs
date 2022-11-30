using UnityEngine;

namespace Assets.Scripts.WorldGen.RandomUpdaters
{
    public interface IRandomUpdater
    {
        void Update(CubeMap map, (Vector3Int pos, Chunk ch) chunk);
    }
}