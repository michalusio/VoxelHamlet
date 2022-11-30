using Assets.Scripts.PathFinding;
using Assets.Scripts.Utilities;
using UnityEngine;

namespace Assets.Scripts.WorldGen.RandomUpdaters
{
    public class GrassSpreadUpdater : IRandomUpdater
    {
        public void Update(CubeMap map, (Vector3Int pos, Chunk ch) chunk)
        {
            if (!chunk.ch.HasAnyBlock()) return;
            for (int i = 0; i < GlobalSettings.Variables["GrassSpreadSpeed"].AsInt(); i++)
            {
                var randomPos = new Vector3Int(Random.Range(0, CubeMap.RegionSize), Random.Range(0, CubeMap.RegionSize), Random.Range(0, CubeMap.RegionSize));
                Block b = chunk.ch[randomPos];
                if (b.BlockType != BlockType.Dirt) continue;

                var upPos = randomPos.GetBlockAtFace(BlockDirection.TOP);
                bool isEmptyTop = !map.IsInBounds(upPos) || map[upPos + chunk.pos].IsWalkable();
                if (!isEmptyTop) continue;

                var grassRandomPos = randomPos + chunk.pos + new Vector3Int(Random.Range(-2, 3), Random.Range(-2, 3), Random.Range(-2, 3));
                if (map.IsInBounds(grassRandomPos) && map[grassRandomPos].BlockType == BlockType.Grass)
                {
                    b.BlockType = BlockType.Grass;
                    chunk.ch[randomPos] = b;
                }
            }
        }
    }
}
