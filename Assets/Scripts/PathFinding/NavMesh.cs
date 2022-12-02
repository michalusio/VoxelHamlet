using Assets.Scripts.Utilities;
using Assets.Scripts.WorldGen;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.PathFinding
{
    public class NavMesh: MonoBehaviour
    {
        [Range(0f, 1f)]
        public float MeshOpacity = 0;

        private Dictionary<Vector3Int, NavMeshChunk> NavChunks = new Dictionary<Vector3Int, NavMeshChunk>();

        void Start()
        {
            var navMeshWatcher = System.Diagnostics.Stopwatch.StartNew();
#if UNITY_EDITOR
            if (!NavMeshChunk.QuadMesh) return;
#endif
            NavChunks = GlobalSettings.Instance.Map.GetChunks
                .ToDictionary(kv => kv.Key * CubeMap.RegionSize, kv => new NavMeshChunk(kv.Key * CubeMap.RegionSize, kv.Value));
            navMeshWatcher.Stop();
            Debug.Log($"NavMesh generation time: {navMeshWatcher.ElapsedMilliseconds}ms");
            Debug.Log($"NavMesh planes: {NavChunks.Values.Select(v => v.NavMeshPlanes.Count).Sum()}");
        }

        public void NotifyBlockChanged(Vector3Int pos)
        {
            if (NavChunks.Count == 0) return;
            var regionSizeShift = CubeMap.RegionSizeShift;
            var chunkKey = new Vector3Int(
                (pos.x >> regionSizeShift) << regionSizeShift,
                (pos.y >> regionSizeShift) << regionSizeShift,
                (pos.z >> regionSizeShift) << regionSizeShift
            );
            var y = pos.y - chunkKey.y;
            var chunk = NavChunks[chunkKey];
            chunk.NotifyBlockChanged(pos);
            if (y == 0 && NavChunks.TryGetValue(chunkKey + Vector3Int.down * CubeMap.RegionSize, out chunk))
            {
                chunk.NotifyBlockChanged(pos);
            }
            if (y == CubeMap.RegionSize - 1 && NavChunks.TryGetValue(chunkKey + Vector3Int.up * CubeMap.RegionSize, out chunk))
            {
                chunk.NotifyBlockChanged(pos);
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (MeshOpacity < 0.01f) return;
            var regionSizeShift = CubeMap.RegionSizeShift;
            var regionSize = 1 << regionSizeShift;
            var cameraPos = Camera.main.transform.position;
            var cameraChunkKey = new Vector3Int(Mathf.FloorToInt(cameraPos.x) >> regionSizeShift, Mathf.FloorToInt(cameraPos.y) >> regionSizeShift, Mathf.FloorToInt(cameraPos.z) >> regionSizeShift);
            var diffKey = new Vector3Int();
            for (int x = -2; x <= 2; x++)
            {
                diffKey.x = x;
                for (int z = -2; z <= 2; z++)
                {
                    diffKey.z = z;
                    for (int y = -2; y <= 2; y++)
                    {
                        diffKey.y = y;
                        var chunkKey = (cameraChunkKey + diffKey) * regionSize;
                        if (!NavChunks.TryGetValue(chunkKey, out var chunk)) continue;
                        chunk.RenderGizmo(MeshOpacity);
                    }
                }
            }
        }
#endif
    }
}
