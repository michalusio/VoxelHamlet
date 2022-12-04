using Assets.Scripts.ConfigScripts;
using Assets.Scripts.Utilities;
using Assets.Scripts.WorldGen.RandomUpdaters;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.Scripts.WorldGen
{
    public class CubeMap
    {
        public static readonly int RegionSizeShift = GlobalSettings.Variables["ChunkSizeShift"].AsInt();
        public static readonly int RegionSize = 1 << RegionSizeShift;
        public static readonly int RegionSizeSquared = RegionSize * RegionSize;
        public static readonly int RegionSizeCubed = RegionSizeSquared * RegionSize;

        public readonly int W;
        public readonly int H;
        public readonly int D;

        private readonly Dictionary<Vector3Int, Chunk> Chunks;
        public IReadOnlyDictionary<Vector3Int, Chunk> GetChunks => Chunks;

        private readonly Dictionary<Vector3Int, Mesh> Meshes;

        public readonly List<IRandomUpdater> Updaters;

        private readonly List<(DataMesh chunkMesh, Vector3Int position)> dataMeshes;

        public CubeMap(int w, int h, int d)
        {
            W = RegionSize * ((w + RegionSize - 1) / RegionSize);
            H = RegionSize * ((h + RegionSize - 1) / RegionSize);
            D = RegionSize * ((d + RegionSize - 1) / RegionSize);
            Chunks = new Dictionary<Vector3Int, Chunk>();
            Meshes = new Dictionary<Vector3Int, Mesh>();
            Updaters = new List<IRandomUpdater>();
            dataMeshes = new List<(DataMesh chunkMesh, Vector3Int position)>();
            for (int x = 0; x < W / RegionSize; x++)
            {
                for (int y = 0; y < H / RegionSize; y++)
                {
                    for (int z = 0; z < D / RegionSize; z++)
                    {
                        Chunks[new Vector3Int(x, y, z)] = new Chunk(this);
                    }
                }
            }
            for (int x = 0; x < W / RegionSize; x++)
            {
                for (int y = 0; y < H / RegionSize; y++)
                {
                    for (int z = 0; z < D / RegionSize; z++)
                    {
                        var key = new Vector3Int(x, y, z);
                        Chunks[key].Init(key);
                    }
                }
            }
        }

        public Block this[int x, int y, int z]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Vector3Int chPos = default;
                chPos.x = x >> RegionSizeShift;
                chPos.y = y >> RegionSizeShift;
                chPos.z = z >> RegionSizeShift;
                if (Chunks.TryGetValue(chPos, out var chunk))
                {
                    var r = RegionSize - 1;
                    return chunk[x & r, y & r, z & r];
                } else
                {
                    return default;
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                Vector3Int chPos = default;
                chPos.x = x >> RegionSizeShift;
                chPos.y = y >> RegionSizeShift;
                chPos.z = z >> RegionSizeShift;
                var r = RegionSize - 1;
                Chunks[chPos][x & r, y & r, z & r] = value;
                GlobalSettings.Instance.NavMesh?.NotifyBlockChanged(new Vector3Int(x, y, z));
            }
        }

        public Block this[Vector3Int pos]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this[pos.x, pos.y, pos.z];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => this[pos.x, pos.y, pos.z] = value;
        }

        public void Clear(BlockType type = BlockType.Air)
        {
            Parallel.ForEach(Chunks, ch =>
            {
                ch.Value.Dirty = true;
                ch.Value.Clear(type);
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool IsInBounds(Vector3Int blockPos)
        {
            return blockPos.x >= 0 && blockPos.x < W && blockPos.y >= 0 && blockPos.y < H && blockPos.z >= 0 && blockPos.z < D;
        }

        public void SetCube(Vector3Int minPosition, Vector3Int size, Func<Block, Vector3Int, BlockType> action)
        {
            Vector3Int v = default;

            for (int x = Mathf.Max(0, minPosition.x); x < Mathf.Min(W, minPosition.x + size.x); x++)
            {
                v.x = x;
                for (int y = Mathf.Max(0, minPosition.y); y < Mathf.Min(H, minPosition.y + size.y); y++)
                {
                    v.y = y;
                    for (int z = Mathf.Max(0, minPosition.z); z < Mathf.Min(D, minPosition.z + size.z); z++)
                    {
                        v.z = z;
                        var block = this[x, y, z];
                        block.BlockType = action(block, v);
                        this[x, y, z] = block;
                    }
                }
            }

            for (int x = Mathf.Max(0, minPosition.x >> RegionSizeShift); x <= (Mathf.Min(W, minPosition.x + size.x) - 1) >> RegionSizeShift; x++)
            {
                v.x = x;
                for (int y = Mathf.Max(0, minPosition.y >> RegionSizeShift); y <= (Mathf.Min(H, minPosition.y + size.y) - 1) >> RegionSizeShift; y++)
                {
                    v.y = y;
                    for (int z = Mathf.Max(0, minPosition.z >> RegionSizeShift); z <= (Mathf.Min(D, minPosition.z + size.z) - 1) >> RegionSizeShift; z++)
                    {
                        v.z = z;
                        Chunks[v].Dirty = true;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveCube(Vector3Int minPosition, Vector3Int size)
        {
            SetCube(minPosition, size, (b, v) => BlockType.Air);
        }

        public int GetHighestYAt(int x, int z)
        {
            var r = RegionSize - 1;
            var chunkX = x >> RegionSizeShift;
            var chunkZ = z >> RegionSizeShift;
            var blockX = x & r;
            var blockZ = z & r;
            for (int y = (H - 1) >> RegionSizeShift; y >= 0; y--)
            {
                Vector3Int v = default;
                v.x = chunkX;
                v.y = y;
                v.z = chunkZ;
                var chunk = Chunks[v];
                if (chunk.HasAnyBlock())
                {
                    for (int blockY = RegionSize - 1; blockY >= 0; blockY--)
                    {
                        if (chunk[blockX, blockY, blockZ].BlockType != BlockType.Air)
                        {
                            return blockY + (y << RegionSizeShift);
                        }
                    }
                }
            }

            return 0;
        }

        /// <summary>
        /// Runs the block updaters for each chunk.
        /// </summary>
        public void RunUpdaters()
        {
            foreach (var kv in Chunks)
            {
                foreach (var u in Updaters)
                {
                    u.Update(this, (kv.Key, kv.Value));
                }
            }
        }

        /// <summary>
        /// Updates all meshes that have changes pending.
        /// </summary>
        public void UpdateMeshes()
        {
            var updateWatcher = System.Diagnostics.Stopwatch.StartNew();
            dataMeshes.Clear();

            foreach (var kv in Chunks)
            {
                if (kv.Value.Dirty)
                {
                    kv.Value.Dirty = false;
                    dataMeshes.Add((kv.Value.GenerateDataMesh(), kv.Key * RegionSize));
                }
            }
            if (dataMeshes.Count > 0) DynamicLogger.Log("CubeMap", $"Generated {dataMeshes.Count} meshes data in {updateWatcher.ElapsedMilliseconds}ms");
            updateWatcher.Restart();

            var vertexDescriptor = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.UInt8, 4);

            var size = new Vector3(RegionSize, RegionSize, RegionSize);

            var bounds = new Bounds(size / 2, size);

            foreach (var (chunkMesh, position) in dataMeshes)
            {
                Meshes.TryGetValue(position, out Mesh mesh);
                LoadChunkIntoMesh(ref mesh, chunkMesh.vertexList, chunkMesh.indexList, ref vertexDescriptor, ref bounds);
                Meshes[position] = mesh;
            }
            if (dataMeshes.Count > 0) DynamicLogger.Log("CubeMap", $"Updated {dataMeshes.Count} meshes in {updateWatcher.ElapsedMilliseconds}ms");
            updateWatcher.Stop();
        }

        /// <summary>
        /// Loads the chunk data into a specified mesh.
        /// </summary>
        private void LoadChunkIntoMesh(ref Mesh m, List<ChunkVertex> vertexList, List<ushort> indexList, ref VertexAttributeDescriptor descriptor, ref Bounds bounds)
        {
            if (vertexList.Count > 0)
            {
                if (!m)
                {
                    m = new Mesh();
                    m.MarkDynamic();
                    m.bounds = bounds;
                }
                m.SetVertexBufferParams(vertexList.Count, descriptor);
                m.SetVertexBufferData(vertexList, 0, 0, vertexList.Count, flags: (MeshUpdateFlags)15);
                m.SetIndices(indexList, MeshTopology.Quads, 0, false);
                m.UploadMeshData(false);
            }
            else if (m) m.Clear(false);
        }

        internal void DrawMeshes(Camera camera, Material material, string layerName = "Terrain", Vector3 offset = default, bool shadows = true)
        {
            var renderRange = GlobalSettings.Variables["RenderRange"].AsInt();
            var layer = LayerMask.NameToLayer(layerName);
            var cameraPos = camera.transform.position - offset;
            var cameraChunkKey = new Vector3Int(Mathf.FloorToInt(cameraPos.x) >> RegionSizeShift, Mathf.FloorToInt(cameraPos.y) >> RegionSizeShift, Mathf.FloorToInt(cameraPos.z) >> RegionSizeShift);
            var diffKey = new Vector3Int();
            for (int x = -renderRange; x <= renderRange; x++)
            {
                diffKey.x = x;
                for (int z = -renderRange; z <= renderRange; z++)
                {
                    diffKey.z = z;
                    for (int y = -renderRange; y <= renderRange; y++)
                    {
                        diffKey.y = y;
                        var chunkKey = (cameraChunkKey + diffKey) * RegionSize;
                        if (!Meshes.TryGetValue(chunkKey, out var mesh)) continue;
                        var matrix = Matrix4x4.Translate(chunkKey + offset);
                        Graphics.DrawMesh(mesh, matrix, material, layer, camera, 0, null, shadows ? ShadowCastingMode.TwoSided : ShadowCastingMode.Off, shadows);
                    }
                }
            }
        }
    }
}