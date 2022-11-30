using Assets.Scripts.WorldGen;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Utilities
{
    public class BlockSnapshot
    {
        private readonly List<GreedyCube> CompressedBlocks;
        private readonly List<int> ChunkGreedyIndex;
        private readonly List<Vector3Int> ChunkPos;
        private Vector2Int Size;

        public BlockSnapshot(CubeMap editMap, Vector2Int size)
        {
            CompressedBlocks = new List<GreedyCube>();
            ChunkGreedyIndex = new List<int>();
            ChunkPos = new List<Vector3Int>();
            foreach (var ch in editMap.GetChunks)
            {
                ChunkPos.Add(ch.Key);
                CompressedBlocks.AddRange(ch.Value.GetGreedyCubes());
                ChunkGreedyIndex.Add(CompressedBlocks.Count);
            }
            Size = size;
        }

        internal void Load(CubeMap editMap)
        {
            GlobalSettings.Instance.EditMode.EditSize = Size;
            editMap.Clear(BlockType.Air);
            int lastIndex = 0;
            for (int i = 0; i < ChunkGreedyIndex.Count; i++)
            {
                var index = ChunkGreedyIndex[i];
                var startPos = ChunkPos[i] * CubeMap.RegionSize;
                for (int gIndex = lastIndex; gIndex < index; gIndex++)
                {
                    var greedyCube = CompressedBlocks[gIndex];
                    for (int x = greedyCube.sx; x <= greedyCube.ex; x++)
                    {
                        for (int y = greedyCube.sy; y <= greedyCube.ey; y++)
                        {
                            for (int z = greedyCube.sz; z <= greedyCube.ez; z++)
                            {
                                Vector3Int finalPos = default;
                                finalPos.x = startPos.x + x;
                                finalPos.y = startPos.y + y;
                                finalPos.z = startPos.z + z;
                                var b = editMap[finalPos];
                                b.BlockType = greedyCube.id;
                                editMap[finalPos] = b;
                            }
                        }
                    }
                }
                lastIndex = index;
            }
        }
    }
}
