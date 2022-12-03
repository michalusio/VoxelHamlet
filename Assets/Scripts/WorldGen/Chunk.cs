using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Assets.Scripts.WorldGen
{
    public class Chunk
    {
        private static readonly bool[] alreadyMeshed = new bool[CubeMap.RegionSizeCubed];
        private Block[] Blocks;
        private readonly byte[] airAround;

        private readonly List<ChunkVertex> vertexList;
        private readonly List<int> indexList;

        private readonly CubeMap map;
        private Vector3Int Position;

        public Chunk ChunkPX { get; private set; }
        public Chunk ChunkNX { get; private set; }
        public Chunk ChunkPZ { get; private set; }
        public Chunk ChunkNZ { get; private set; }
        public Chunk ChunkPY { get; private set; }
        public Chunk ChunkNY { get; private set; }

        public bool Dirty { get; set; }

        public Chunk(CubeMap cubeMap)
        {
            map = cubeMap;
            vertexList = new List<ChunkVertex>(CubeMap.RegionSizeSquared >> 1);
            indexList = new List<int>(CubeMap.RegionSizeSquared >> 1);
            airAround = new byte[CubeMap.RegionSizeCubed];
            for (int a = 0; a < CubeMap.RegionSizeCubed; a += 4)
            {
                airAround[a] = 63;
                airAround[a + 1] = 63;
                airAround[a + 2] = 63;
                airAround[a + 3] = 63;
            }
        }

        internal void Init(Vector3Int key)
        {
            Position = key;
            key.x--;
            if (map.GetChunks.TryGetValue(key, out var chunk))
            {
                ChunkNX = chunk;
            }
            key.x += 2;
            if (map.GetChunks.TryGetValue(key, out chunk))
            {
                ChunkPX = chunk;
            }
            key.x--;

            key.y--;
            if (map.GetChunks.TryGetValue(key, out chunk))
            {
                ChunkNY = chunk;
            }
            key.y += 2;
            if (map.GetChunks.TryGetValue(key, out chunk))
            {
                ChunkPY = chunk;
            }
            key.y--;

            key.z--;
            if (map.GetChunks.TryGetValue(key, out chunk))
            {
                ChunkNZ = chunk;
            }
            key.z += 2;
            if (map.GetChunks.TryGetValue(key, out chunk))
            {
                ChunkPZ = chunk;
            }
        }

        public Block this[int x, int y, int z]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Blocks == null ? default : Blocks[(((z << CubeMap.RegionSizeShift) + y) << CubeMap.RegionSizeShift) + x];
            set
            {
                Blocks ??= new Block[CubeMap.RegionSizeCubed];
                var index = (((z << CubeMap.RegionSizeShift) + y) << CubeMap.RegionSizeShift) + x;
                var previousValue = Blocks[index];
                Blocks[index] = value;

                if ((previousValue.BlockType == BlockType.Air && value.BlockType != BlockType.Air) ||
                    (previousValue.BlockType != BlockType.Air && value.BlockType == BlockType.Air))
                {
                    if (x > 0) airAround[index - 1] ^= 1; else if (ChunkNX != null) ChunkNX.airAround[index + CubeMap.RegionSize - 1] ^= 1;
                    if (y > 0) airAround[index - CubeMap.RegionSize] ^= 2; else if (ChunkNY != null) ChunkNY.airAround[index + CubeMap.RegionSizeSquared - CubeMap.RegionSize] ^= 2;
                    if (z > 0) airAround[index - CubeMap.RegionSizeSquared] ^= 4; else if (ChunkNZ != null) ChunkNZ.airAround[index + CubeMap.RegionSizeCubed - CubeMap.RegionSizeSquared] ^= 4;
                    if (x < CubeMap.RegionSize - 1) airAround[index + 1] ^= 8; else if (ChunkPX != null) ChunkPX.airAround[index - CubeMap.RegionSize + 1] ^= 8;
                    if (y < CubeMap.RegionSize - 1) airAround[index + CubeMap.RegionSize] ^= 16; else if (ChunkPY != null) ChunkPY.airAround[index - CubeMap.RegionSizeSquared + CubeMap.RegionSize] ^= 16;
                    if (z < CubeMap.RegionSize - 1) airAround[index + CubeMap.RegionSizeSquared] ^= 32; else if (ChunkPZ != null) ChunkPZ.airAround[index - CubeMap.RegionSizeCubed + CubeMap.RegionSizeSquared] ^= 32;
                }

                Dirty = true;
            }
        }

        public Block this[Vector3Int pos]
        {
            get => this[pos.x, pos.y, pos.z];
            set => this[pos.x, pos.y, pos.z] = value;
        }

        public DataMesh GenerateDataMesh()
        {
            vertexList.Clear();
            indexList.Clear();

            if (Blocks != null)
            {
                Array.Clear(alreadyMeshed, 0, alreadyMeshed.Length);
                var index = 0;
                for (var z = 0; z < CubeMap.RegionSize; z++)
                {
                    for (var y = 0; y < CubeMap.RegionSize; y++)
                    {
                        for (var x = 0; x < CubeMap.RegionSize; x++)
                        {
                            if (Blocks[index].BlockType != BlockType.Air)
                            {
                                if (!alreadyMeshed[index])
                                {
                                    GreedyCube cube = default;
                                    TryGreedyMesh(ref cube, Blocks, x, y, z);
                                    cube.BlockMesh(alreadyMeshed);
                                    if (cube.id != BlockType.Entity)
                                    {
                                        cube.ex++;
                                        cube.ey++;
                                        cube.ez++;
                                        cube.ToNormalBlock(vertexList, indexList, Position.y == 0);
                                    }
                                }
                            }
                            index++;
                        }
                    }
                }
            }

            return new DataMesh(
                vertexList,
                indexList
            );
        }

        public IReadOnlyList<GreedyCube> GetGreedyCubes()
        {
            var cubes = new List<GreedyCube>();
            if (Blocks != null)
            {
                Array.Clear(alreadyMeshed, 0, alreadyMeshed.Length);
                var index = 0;
                for (var z = 0; z < CubeMap.RegionSize; z++)
                {
                    for (var y = 0; y < CubeMap.RegionSize; y++)
                    {
                        for (var x = 0; x < CubeMap.RegionSize; x++)
                        {
                            if (Blocks[index].BlockType != BlockType.Air)
                            {
                                if (!alreadyMeshed[index])
                                {
                                    GreedyCube cube = default;
                                    TryGreedyMesh(ref cube, Blocks, x, y, z);
                                    cube.BlockMesh(alreadyMeshed);
                                    cubes.Add(cube);
                                }
                            }
                            index++;
                        }
                    }
                }
            }
            return cubes.AsReadOnly();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TryGreedyMesh(ref GreedyCube cube, Block[] blocks, int sx, int sy, int sz)
        {
            var startIndex = (((sz << CubeMap.RegionSizeShift) + sy) << CubeMap.RegionSizeShift) + sx;
            var b = blocks[startIndex];
            cube.sx = (byte)sx;
            cube.sy = (byte)sy;
            cube.sz = (byte)sz;
            cube.id = b.BlockType;
            cube.ORFaceCount = airAround[startIndex];
            cube.ez = (byte)sz;
            for (var z = sz + 1; z < CubeMap.RegionSize; z ++)
            {
                var index = (((z << CubeMap.RegionSizeShift) + sy) << CubeMap.RegionSizeShift) + sx;
                cube.ORFaceCount |= airAround[index];
                if (alreadyMeshed[index] || (airAround[index] > 0 && blocks[index].BlockType != cube.id))
                {
                    break;
                }
                cube.ez = (byte)z;
            }

            cube.ey = (byte)sy;
            for (var y = sy + 1; y < CubeMap.RegionSize; y ++)
            {
                bool passed = true;
                var orFace = 0;
                for (var z = sz; z <= cube.ez; z ++)
                {
                    var index = (((z << CubeMap.RegionSizeShift) + y) << CubeMap.RegionSizeShift) + sx;
                    orFace |= airAround[index];
                    if (alreadyMeshed[index] || (airAround[index] > 0 && blocks[index].BlockType != cube.id))
                    {
                        passed = false;
                        break;
                    }
                }
                if (passed)
                {
                    cube.ey = (byte)y;
                    cube.ORFaceCount |= (byte)orFace;
                }
                else break;
            }

            cube.ex = (byte)sx;
            for (var x = sx + 1; x < CubeMap.RegionSize; x ++)
            {
                var orFace = 0;
                for (var y = sy; y <= cube.ey; y ++)
                {
                    for (var z = sz; z <= cube.ez; z ++)
                    {
                        var index = (((z << CubeMap.RegionSizeShift) + y) << CubeMap.RegionSizeShift) + x;
                        orFace |= airAround[index];
                        if (alreadyMeshed[index] || (airAround[index] > 0 && blocks[index].BlockType != cube.id))
                        {
                            return;
                        }
                    }
                }
                cube.ex = (byte)x;
                cube.ORFaceCount |= (byte)orFace;
            }
        }

        public void Clear(BlockType type = BlockType.Air)
        {
            if (type == BlockType.Air)
            {
                Blocks = null;
            }
            else
            {
                Blocks ??= new Block[CubeMap.RegionSizeCubed];
                Block block = new Block
                {
                    BlockType = type
                };
                for (int index = 0; index < CubeMap.RegionSizeCubed; index+=4)
                {
                    Blocks[index] = block;
                    Blocks[index + 1] = block;
                    Blocks[index + 2] = block;
                    Blocks[index + 3] = block;
                }
            }
            for (int a = 0; a < CubeMap.RegionSizeCubed; a += 4)
            {
                airAround[a] = 63;
                airAround[a + 1] = 63;
                airAround[a + 2] = 63;
                airAround[a + 3] = 63;
            }
        }

        public bool HasAnyBlock()
        {
            return Blocks != null && (vertexList.Count > 0 || Dirty);
        }
    }
}