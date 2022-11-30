using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Assets.Scripts.WorldGen
{
    public partial class Chunk
    {
        private Block[] Blocks;
        private bool[] alreadyMeshed;
        private readonly byte[] airAround;

        private readonly List<ChunkVertex> vertexListPX, vertexListNX, vertexListPY, vertexListNY, vertexListPZ, vertexListNZ;
        private readonly List<int> indexListPX, indexListNX, indexListPY, indexListNY, indexListPZ, indexListNZ;
        private readonly CubeMap map;

        private Chunk chunkPX, chunkNX, chunkPZ, chunkNZ, chunkPY, chunkNY;

        public bool Dirty { get; set; }

        public Chunk(CubeMap cubeMap)
        {
            map = cubeMap;
            vertexListPX = new List<ChunkVertex>(CubeMap.RegionSize);
            vertexListNX = new List<ChunkVertex>(CubeMap.RegionSize);
            vertexListPY = new List<ChunkVertex>(CubeMap.RegionSize);
            vertexListNY = new List<ChunkVertex>(CubeMap.RegionSize);
            vertexListPZ = new List<ChunkVertex>(CubeMap.RegionSize);
            vertexListNZ = new List<ChunkVertex>(CubeMap.RegionSize);

            indexListPX = new List<int>(CubeMap.RegionSize);
            indexListNX = new List<int>(CubeMap.RegionSize);
            indexListPY = new List<int>(CubeMap.RegionSize);
            indexListNY = new List<int>(CubeMap.RegionSize);
            indexListPZ = new List<int>(CubeMap.RegionSize);
            indexListNZ = new List<int>(CubeMap.RegionSize);
            airAround = new byte[CubeMap.RegionSizeCubed];
            for (int a = 0; a < CubeMap.RegionSizeCubed; a++)
            {
                airAround[a] = 63;
            }
        }

        internal void Init(Vector3Int key)
        {
            key.x--;
            map.GetChunks.TryGetValue(key, out chunkNX);
            key.x += 2;
            map.GetChunks.TryGetValue(key, out chunkPX);
            
            key.x--;
            key.y--;
            map.GetChunks.TryGetValue(key, out chunkNY);
            key.y += 2;
            map.GetChunks.TryGetValue(key, out chunkPY);

            key.y--;
            key.z--;
            map.GetChunks.TryGetValue(key, out chunkNZ);
            key.z += 2;
            map.GetChunks.TryGetValue(key, out chunkPZ);
        }

        public Block this[int x, int y, int z]
        {
            get => Blocks == null ? default : Blocks[(((z << CubeMap.RegionSizeShift) + y) << CubeMap.RegionSizeShift) + x];
            set
            {
                if (Blocks == null)
                {
                    alreadyMeshed = new bool[CubeMap.RegionSizeCubed];
                    Blocks = new Block[CubeMap.RegionSizeCubed];
                }
                var index = (((z << CubeMap.RegionSizeShift) + y) << CubeMap.RegionSizeShift) + x;
                var previousValue = Blocks[index];
                Blocks[index] = value;

                if ((previousValue.BlockType == BlockType.Air && value.BlockType != BlockType.Air) ||
                    (previousValue.BlockType != BlockType.Air && value.BlockType == BlockType.Air))
                {
                    if (x > 0) airAround[index - 1] ^= 1; else if (chunkNX != null) chunkNX.airAround[index + CubeMap.RegionSize - 1] ^= 1;
                    if (y > 0) airAround[index - CubeMap.RegionSize] ^= 2; else if (chunkNY != null) chunkNY.airAround[index + CubeMap.RegionSizeSquared - CubeMap.RegionSize] ^= 2;
                    if (z > 0) airAround[index - CubeMap.RegionSizeSquared] ^= 4; else if (chunkNZ != null) chunkNZ.airAround[index + CubeMap.RegionSizeCubed - CubeMap.RegionSizeSquared] ^= 4;
                    if (x < CubeMap.RegionSize - 1) airAround[index + 1] ^= 8; else if (chunkPX != null) chunkPX.airAround[index - CubeMap.RegionSize + 1] ^= 8;
                    if (y < CubeMap.RegionSize - 1) airAround[index + CubeMap.RegionSize] ^= 16; else if (chunkPY != null) chunkPY.airAround[index - CubeMap.RegionSizeSquared + CubeMap.RegionSize] ^= 16;
                    if (z < CubeMap.RegionSize - 1) airAround[index + CubeMap.RegionSizeSquared] ^= 32; else if (chunkPZ != null) chunkPZ.airAround[index - CubeMap.RegionSizeCubed + CubeMap.RegionSizeSquared] ^= 32;
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
            vertexListPX.Clear();
            vertexListNX.Clear();
            vertexListPY.Clear();
            vertexListNY.Clear();
            vertexListPZ.Clear();
            vertexListNZ.Clear();
                       
            indexListPX.Clear();
            indexListNX.Clear();
            indexListPY.Clear();
            indexListNY.Clear();
            indexListPZ.Clear();
            indexListNZ.Clear();

            if (Blocks != null)
            {
                var index = 0;
                for (byte z = 0; z < CubeMap.RegionSize; z++)
                {
                    for (byte y = 0; y < CubeMap.RegionSize; y++)
                    {
                        for (byte x = 0; x < CubeMap.RegionSize; x++)
                        {
                            if (Blocks[index].BlockType != BlockType.Air)
                            {
                                if (!alreadyMeshed[index])
                                {
                                    GreedyCube cube = default;
                                    TryGreedyMesh(ref cube, x, y, z);
                                    cube.BlockMesh(alreadyMeshed);
                                    if (cube.id != BlockType.Entity)
                                    {
                                        cube.ex++;
                                        cube.ey++;
                                        cube.ez++;
                                        AddNormalBlock(ref cube);
                                    }
                                }
                                alreadyMeshed[index] = false;
                            }
                            index++;
                        }
                    }
                }
            }

            return new DataMesh(
                vertexListPX, vertexListNX,
                vertexListPY, vertexListNY,
                vertexListPZ, vertexListNZ,
                indexListPX, indexListNX,
                indexListPY, indexListNY,
                indexListPZ, indexListNZ);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddNormalBlock(ref GreedyCube cube)
        {
            var countPX = vertexListPX.Count;
            var countPY = vertexListPY.Count;
            var countPZ = vertexListPZ.Count;
            var countNX = vertexListNX.Count;
            var countNY = vertexListNY.Count;
            var countNZ = vertexListNZ.Count;

            ChunkVertex v000 = default;
            ChunkVertex v001 = default;
            ChunkVertex v010 = default;
            ChunkVertex v011 = default;
            ChunkVertex v100 = default;
            ChunkVertex v101 = default;
            ChunkVertex v110 = default;
            ChunkVertex v111 = default;

            v000.x = v001.x = v010.x = v011.x = cube.sx;
            v000.y = v001.y = v100.y = v101.y = cube.sy;
            v000.z = v010.z = v100.z = v110.z = cube.sz;

            v100.x = v101.x = v110.x = v111.x = cube.ex;
            v010.y = v011.y = v110.y = v111.y = cube.ey;
            v001.z = v011.z = v101.z = v111.z = cube.ez;

            v000.blockIndex = v001.blockIndex = v010.blockIndex = v011.blockIndex =
            v100.blockIndex = v101.blockIndex = v110.blockIndex = v111.blockIndex = (byte)cube.id;
            if ((cube.ORFaceCount & 8) > 0)
            {
                vertexListNX.Add(v000);
                vertexListNX.Add(v010);
                vertexListNX.Add(v001);
                vertexListNX.Add(v011);
                indexListNX.Add(countNX + 2);
                indexListNX.Add(countNX + 3);
                indexListNX.Add(countNX + 1);
                indexListNX.Add(countNX);
            }
            if ((cube.ORFaceCount & 16) > 0)
            {
                vertexListNY.Add(v000);
                vertexListNY.Add(v100);
                vertexListNY.Add(v001);
                vertexListNY.Add(v101);
                indexListNY.Add(countNY);
                indexListNY.Add(countNY + 1);
                indexListNY.Add(countNY + 3);
                indexListNY.Add(countNY + 2);
            }
            if ((cube.ORFaceCount & 32) > 0)
            {
                vertexListNZ.Add(v000);
                vertexListNZ.Add(v100);
                vertexListNZ.Add(v010);
                vertexListNZ.Add(v110);
                indexListNZ.Add(countNZ + 2);
                indexListNZ.Add(countNZ + 3);
                indexListNZ.Add(countNZ + 1);
                indexListNZ.Add(countNZ);
            }
            if ((cube.ORFaceCount & 1) > 0)
            {
                vertexListPX.Add(v100);
                vertexListPX.Add(v110);
                vertexListPX.Add(v101);
                vertexListPX.Add(v111);
                indexListPX.Add(countPX);
                indexListPX.Add(countPX + 1);
                indexListPX.Add(countPX + 3);
                indexListPX.Add(countPX + 2);
            }
            if ((cube.ORFaceCount & 2) > 0)
            {
                vertexListPY.Add(v010);
                vertexListPY.Add(v110);
                vertexListPY.Add(v011);
                vertexListPY.Add(v111);
                indexListPY.Add(countPY + 2);
                indexListPY.Add(countPY + 3);
                indexListPY.Add(countPY + 1);
                indexListPY.Add(countPY);
            }
            if ((cube.ORFaceCount & 4) > 0)
            {
                vertexListPZ.Add(v001);
                vertexListPZ.Add(v101);
                vertexListPZ.Add(v011);
                vertexListPZ.Add(v111);
                indexListPZ.Add(countPZ);
                indexListPZ.Add(countPZ + 1);
                indexListPZ.Add(countPZ + 3);
                indexListPZ.Add(countPZ + 2);
            }

        }

        public GreedyCube[] GetGreedyCubes()
        {
            var cubes = new List<GreedyCube>();
            if (Blocks != null)
            {
                var index = 0;
                for (byte z = 0; z < CubeMap.RegionSize; z++)
                {
                    for (byte y = 0; y < CubeMap.RegionSize; y++)
                    {
                        for (byte x = 0; x < CubeMap.RegionSize; x++)
                        {
                            if (Blocks[index].BlockType != BlockType.Air)
                            {
                                if (!alreadyMeshed[index])
                                {
                                    GreedyCube cube = default;
                                    TryGreedyMesh(ref cube, x, y, z);
                                    cube.BlockMesh(alreadyMeshed);
                                    cubes.Add(cube);
                                }
                                alreadyMeshed[index] = false;
                            }
                            index++;
                        }
                    }
                }
            }
            return cubes.ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TryGreedyMesh(ref GreedyCube cube, byte sx, byte sy, byte sz)
        {
            var startIndex = (((sz << CubeMap.RegionSizeShift) + sy) << CubeMap.RegionSizeShift) + sx;
            var b = Blocks[startIndex];
            cube.sx = sx;
            cube.sy = sy;
            cube.sz = sz;
            cube.id = b.BlockType;
            cube.entity = b.EntityInBlock;
            cube.ORFaceCount = airAround[startIndex];
            cube.ez = sz;
            for (byte z = (byte)(sz + 1); z < CubeMap.RegionSize; z++)
            {
                var index = (((z << CubeMap.RegionSizeShift) + sy) << CubeMap.RegionSizeShift) + sx;
                cube.ORFaceCount |= airAround[index];
                if (alreadyMeshed[index] || (airAround[index] > 0 && Blocks[index].BlockType != cube.id))
                {
                    break;
                }
                cube.ez = z;
            }

            cube.ey = sy;
            for (byte y = (byte)(sy + 1); y < CubeMap.RegionSize; y++)
            {
                bool passed = true;
                byte orFace = 0;
                for (byte z = sz; z <= cube.ez; z++)
                {
                    var index = (((z << CubeMap.RegionSizeShift) + y) << CubeMap.RegionSizeShift) + sx;
                    orFace |= airAround[index];
                    if (alreadyMeshed[index] || (airAround[index] > 0 && Blocks[index].BlockType != cube.id))
                    {
                        passed = false;
                        break;
                    }
                }
                if (passed)
                {
                    cube.ey = y;
                    cube.ORFaceCount |= orFace;
                }
                else break;
            }

            cube.ex = sx;
            for (byte x = (byte)(sx + 1); x < CubeMap.RegionSize; x++)
            {
                byte orFace = 0;
                for (byte y = sy; y <= cube.ey; y++)
                {
                    for (byte z = sz; z <= cube.ez; z++)
                    {
                        var index = (((z << CubeMap.RegionSizeShift) + y) << CubeMap.RegionSizeShift) + x;
                        orFace |= airAround[index];
                        if (alreadyMeshed[index] || (airAround[index] > 0 && Blocks[index].BlockType != cube.id))
                        {
                            return;
                        }
                    }
                }
                cube.ex = x;
                cube.ORFaceCount |= orFace;
            }
        }

        public void Clear(BlockType type = BlockType.Air)
        {
            if (type == BlockType.Air)
            {
                Blocks = null;
                alreadyMeshed = null;
                for (int index = 0; index < CubeMap.RegionSizeCubed; index++)
                {
                    airAround[index] = 63;
                }
            }
            else
            {
                if (Blocks == null)
                {
                    alreadyMeshed = new bool[CubeMap.RegionSizeCubed];
                    Blocks = new Block[CubeMap.RegionSizeCubed];
                    
                }
                var index = 0;
                Block b = default;
                b.BlockType = type;

                for (byte x = 0; x < CubeMap.RegionSize; x++)
                {
                    for (byte y = 0; y < CubeMap.RegionSize; y++)
                    {
                        for (byte z = 0; z < CubeMap.RegionSize; z++)
                        {
                            Blocks[index] = b;
                            airAround[index] = 63;
                            index++;
                        }
                    }
                }
            }
        }

        public bool HasAnyBlock()
        {
            return Blocks != null && ((vertexListPX.Count + vertexListNX.Count + vertexListPY.Count + vertexListNY.Count + vertexListPZ.Count + vertexListNZ.Count) > 0 || Dirty);
        }
    }
}