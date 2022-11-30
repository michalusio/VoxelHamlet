using Assets.Scripts.Utilities;
using Assets.Scripts.WorldGen;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.PathFinding
{
    internal class NavMeshChunk
    {
        public readonly Vector3Int Position;
        public readonly Chunk Chunk;
        public Dictionary<int, List<NavMeshPlane>> NavMeshPlanes = new Dictionary<int, List<NavMeshPlane>>();
        private readonly bool[][] alreadyMeshed;

        public NavMeshChunk(Vector3Int position, Chunk chunk)
        {
            Position = position;
            Chunk = chunk;
            alreadyMeshed = new bool[CubeMap.RegionSize][];
            var yLimit = chunk.HasAnyBlock() ? CubeMap.RegionSize : 1;
            for (byte y = 0; y < yLimit; y++)
            {
                alreadyMeshed[y] = new bool[CubeMap.RegionSizeSquared];
                NavMeshPlanes[y] = new List<NavMeshPlane>();
            }
            Parallel.For(0, yLimit, GeneratePlanes);
        }

        private void GeneratePlanes(int y)
        {
            var planes = NavMeshPlanes[y];
            var map = GlobalSettings.Instance.Map;
            var topped = y + 1 == CubeMap.RegionSize;
            var bottomed = y == 0;
            var maxH = GlobalSettings.Instance.Map.H - 1;
            var index = 0;
            for (byte z = 0; z < CubeMap.RegionSize; z++)
            {
                for (byte x = 0; x < CubeMap.RegionSize; x++)
                {
                    var pos = Position + new Vector3Int(x, y, z);
                    if (Chunk[x, y, z].BlockType == BlockType.Air &&
                        (pos.y == maxH || 
                            (!topped && Chunk[x, y + 1, z].BlockType == BlockType.Air) ||
                            (topped && map[pos + Vector3Int.up].BlockType == BlockType.Air)
                        ) &&
                        (pos.y == 0 ||
                            (!bottomed && Chunk[x, y - 1, z].BlockType != BlockType.Air) ||
                            (bottomed && map[pos + Vector3Int.down].BlockType != BlockType.Air)
                        )
                       )
                    {
                        if (!alreadyMeshed[y][index])
                        {
                            var plane = new NavMeshPlane();
                            TryGreedyMesh(plane, x, (byte)y, z);
                            plane.BlockMesh(alreadyMeshed[y]);
                            planes.Add(plane);
                        }
                    }
                    index++;
                }
            }
        }

        public void NotifyBlockChanged(Vector3Int pos)
        {
            var sizeM1 = CubeMap.RegionSize - 1;
            var pX = pos.x & sizeM1;
            var pZ = pos.z & sizeM1;
            var pY = pos.y & sizeM1;
            var posY = Position.y;
            for (var y = -1; y < 2; y++)
            {
                NavMeshPlanes.TryGetValue(pY + y, out var planes);
                for (int i = planes.Count - 1; i >= 0; i--)
                {
                    var plane = planes[i];
                    if (Mathf.Abs(plane.Y + posY - pos.y) < 2)
                    {
                        if (
                            plane.minZ <= pZ + 1 &&
                            plane.maxZ >= pZ - 1 &&
                            plane.minX <= pX + 1 &&
                            plane.maxX >= pX - 1
                        )
                        {
                            planes.RemoveAt(i);
                            plane.UnblockMesh(alreadyMeshed[plane.Y]);
                            plane.RemoveConnections();
                        }
                    }
                }
            }
            
            if (pY > 0) GeneratePlanes(pY - 1);
            GeneratePlanes(pY);
            if (pY < sizeM1) GeneratePlanes(pY + 1);
        }

        private void TryGreedyMesh(NavMeshPlane plane, byte sx, byte y, byte sz)
        {
            var map = GlobalSettings.Instance.Map;
            var topped = y + 1 == CubeMap.RegionSize;
            var bottomed = y == 0;
            var maxH = map.H - 1;
            plane.minX = sx;
            plane.minZ = sz;
            plane.Y = y;
            plane.maxZ = sz;
            for (byte z = (byte)(sz + 1); z < CubeMap.RegionSize; z++)
            {
                var p = Position + new Vector3Int(sx, y, z);
                var index = (z << CubeMap.RegionSizeShift) + sx;
                if (alreadyMeshed[y][index] || !(
                    Chunk[sx, y, z].BlockType == BlockType.Air &&
                    (p.y == maxH || 
                        (!topped && Chunk[sx, y + 1, z].BlockType == BlockType.Air) ||
                        (topped && map[p + Vector3Int.up].BlockType == BlockType.Air)
                    ) &&
                    (p.y == 0 || 
                        (!bottomed && Chunk[sx, y - 1, z].BlockType != BlockType.Air) ||
                        (bottomed && map[p + Vector3Int.down].BlockType != BlockType.Air)
                    )))
                {
                    break;
                }
                plane.maxZ = z;
            }

            plane.maxX = sx;
            for (byte x = (byte)(sx + 1); x < CubeMap.RegionSize; x++)
            {
                bool passed = true;
                for (byte z = sz; z <= plane.maxZ; z++)
                {
                    var p = Position + new Vector3Int(x, y, z);
                    var index = (z << CubeMap.RegionSizeShift) + x;
                    if (alreadyMeshed[y][index] || !(
                        Chunk[x, y, z].BlockType == BlockType.Air &&
                        (p.y == maxH || 
                            (!topped && Chunk[x, y + 1, z].BlockType == BlockType.Air) ||
                            (topped && map[p + Vector3Int.up].BlockType == BlockType.Air)
                        ) &&
                        (p.y == 0 ||
                            (!bottomed && Chunk[x, y - 1, z].BlockType != BlockType.Air) ||
                            (bottomed && map[p + Vector3Int.down].BlockType != BlockType.Air)
                        )
                    ))
                    {
                        passed = false;
                        break;
                    }
                }
                if (passed)
                {
                    plane.maxX = x;
                }
                else break;
            }
        }

#if UNITY_EDITOR
        public static readonly Mesh QuadMesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");

        public void RenderGizmo(float opacity)
        {
            var color = Gizmos.color;
            Gizmos.color = new Color(0, 0.5f, 1, opacity);
            foreach(var kv in NavMeshPlanes)
            {
                kv.Value.ForEach(plane =>
                {
                    var scale = new Vector3(plane.maxX - plane.minX + 1, plane.maxZ - plane.minZ + 1, 1);
                    var p = Position + new Vector3Int(plane.minX, plane.Y, plane.minZ) + new Vector3(scale.x, scale.z, scale.y) / 2 + Vector3.down * 0.5f;
                    var rot = Quaternion.Euler(90, 0, 0);
                    Gizmos.DrawMesh(QuadMesh, p, rot, scale);
                    Gizmos.DrawWireMesh(QuadMesh, p, rot, scale);
                });
            }
            Gizmos.color = color;
        }
#endif
    }
}
