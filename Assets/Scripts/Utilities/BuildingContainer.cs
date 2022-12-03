using Assets.Scripts.Utilities.Math;
using Assets.Scripts.WorldGen;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Utilities
{
    public class BuildingContainer : ScriptableObject
    {
        [NonSerialized] public Block[] _blocks;
        [NonSerialized] public Texture2D Icon;

        public Block[] Blocks
        {
            get
            {
                if (_blocks == null)
                    DecodeBlocks();
                return _blocks;
            }
        }

        private GreedyCube[] CompressedBlocks;
        private int[] ChunkGreedyIndex;
        private Vector3Int[] ChunkPos;
        public Vector2Int Size;

        private void DecodeBlocks()
        {
            _blocks = new Block[64 * 64 * 64];
            int lastIndex = 0;
            for (int i = 0; i < ChunkGreedyIndex.Length; i++)
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
                                _blocks[finalPos.z + 64 * (finalPos.y + 64 * finalPos.x)].BlockType = greedyCube.id;
                            }
                        }
                    }
                }
                lastIndex = index;
            }
        }

        internal (Vector3, float) GetBlockStatistics()
        {
            var runningAverage = Vector3.zero;
            var runningCount = 0;
            var runningDistrMax = Vector3.zero;

            int lastIndex = 0;
            for (int i = 0; i < ChunkGreedyIndex.Length; i++)
            {
                var index = ChunkGreedyIndex[i];
                var startPos = ChunkPos[i] * CubeMap.RegionSize;
                for (int gIndex = lastIndex; gIndex < index; gIndex++)
                {
                    GreedyCube g = CompressedBlocks[gIndex];

                    var cubeCount = (g.ex - g.sx + 1) * (g.ey - g.sy + 1) * (g.ez - g.sz + 1);
                    var cubeAverage = cubeCount * (new Vector3(g.ex + g.sx, g.ey + g.sy, g.ez + g.sz) / 2 + startPos);
                    runningAverage += cubeAverage;
                    runningCount += cubeCount;
                }
                lastIndex = index;
            }

            runningAverage /= runningCount;

            lastIndex = 0;
            for (int i = 0; i < ChunkGreedyIndex.Length; i++)
            {
                var index = ChunkGreedyIndex[i];
                var startPos = ChunkPos[i] * CubeMap.RegionSize;
                for (int gIndex = lastIndex; gIndex < index; gIndex++)
                {
                    GreedyCube g = CompressedBlocks[gIndex];

                    var cubeMin = new Vector3(g.sx, g.sy, g.sz) + startPos;
                    var cubeMax = new Vector3(g.ex, g.ey, g.ez) + startPos;

                    runningDistrMax = Vector3.Max(runningDistrMax, Vector3.Max((cubeMax - runningAverage).Abs(), (runningAverage - cubeMin).Abs()));
                }
                lastIndex = index;
            }

            return (runningAverage, Mathf.Max(runningDistrMax.x, runningDistrMax.y, runningDistrMax.z));
        }

        internal static BuildingContainer FromMap(CubeMap map, Vector2Int size)
        {
            var building = CreateInstance<BuildingContainer>();
            var greedyList = new List<GreedyCube>();
            var indexList = new List<int>();
            var posList = new List<Vector3Int>();
            foreach (var ch in map.GetChunks)
            {
                posList.Add(ch.Key);
                greedyList.AddRange(ch.Value.GetGreedyCubes());
                indexList.Add(greedyList.Count);
            }
            building.ChunkGreedyIndex = indexList.ToArray();
            building.ChunkPos = posList.ToArray();
            building.CompressedBlocks = greedyList.ToArray();
            building.Size = size;
            return building;
        }

        internal static void LoadByteData(BuildingContainer container, byte[] data)
        {
            var imageLength = BitConverter.ToInt32(data, 0);

            container.Icon = new Texture2D(2, 2, TextureFormat.RGB24, false, false);
            container.Icon.LoadImage(data.Skip(4).Take(imageLength).ToArray());
            container.Icon.Apply();

            container.Size = new Vector2Int(BitConverter.ToInt16(data, imageLength + 4), BitConverter.ToInt16(data, imageLength + 6));

            var chunks = data[imageLength + 8];
            var index = imageLength + 9;

            container.ChunkPos = new Vector3Int[chunks];
            container.ChunkGreedyIndex = new int[chunks];

            for (int i = 0; i < chunks; i++)
            {
                Vector3Int chunkPos = default;
                chunkPos.x = data[index];
                chunkPos.y = data[index + 1];
                chunkPos.z = data[index + 2];
                container.ChunkPos[i] = chunkPos;
                container.ChunkGreedyIndex[i] = BitConverter.ToInt32(data, index + 3);
                index += 7;
            }
            container.CompressedBlocks = new GreedyCube[(data.Length - index) / 7];
            for (int i = 0; i < container.CompressedBlocks.Length; i++)
            {
                GreedyCube cube = default;
                cube.sx = data[index];
                cube.sy = data[index + 1];
                cube.sz = data[index + 2];
                cube.ex = data[index + 3];
                cube.ey = data[index + 4];
                cube.ez = data[index + 5];
                cube.id = (BlockType)data[index + 6];
                index += 7;

                container.CompressedBlocks[i] = cube;
            }
        }

        public byte[] GetByteData()
        {
            var imageBytes = Icon.EncodeToPNG();
            var imageLength = imageBytes.Length;
            var size = BitConverter.GetBytes((ushort)Size.x).Concat(BitConverter.GetBytes((ushort) Size.y));
            var chunks = ChunkPos.Length;
            var chunkDesc = new byte[7 * chunks];
            for (int i = 0; i < chunks; i++)
            {
                var pos = ChunkPos[i];
                chunkDesc[i * 7] = (byte) pos.x;
                chunkDesc[i * 7 + 1] = (byte) pos.y;
                chunkDesc[i * 7 + 2] = (byte) pos.z;
                BitConverter.GetBytes(ChunkGreedyIndex[i]).CopyTo(chunkDesc, i * 7 + 3);
            }
            var cubeData = new byte[CompressedBlocks.Length * 7];
            for(int i = 0; i < CompressedBlocks.Length; i++)
            {
                var cube = CompressedBlocks[i];
                cubeData[i * 7] = cube.sx;
                cubeData[i * 7 + 1] = cube.sy;
                cubeData[i * 7 + 2] = cube.sz;
                cubeData[i * 7 + 3] = cube.ex;
                cubeData[i * 7 + 4] = cube.ey;
                cubeData[i * 7 + 5] = cube.ez;
                cubeData[i * 7 + 6] = (byte) cube.id;
            }
            return BitConverter.GetBytes(imageLength)
                .Concat(imageBytes)
                .Concat(size)
                .Concat(new byte[] { (byte)chunks })
                .Concat(chunkDesc)
                .Concat(cubeData)
                .ToArray();
        }
    }
}