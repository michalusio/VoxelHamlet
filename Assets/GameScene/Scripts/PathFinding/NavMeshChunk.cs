using Assets.Scripts.ConfigScripts;
using Assets.Scripts.WorldGen;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.PathFinding
{
    public class NavMeshChunk
    {
        public readonly Vector3Int Position;
        public Dictionary<int, List<NavMeshPlane>> NavMeshPlanes = new Dictionary<int, List<NavMeshPlane>>();
        public NavMeshChunk ChunkNX, ChunkNZ; //Should probably add upper and lower chunks too, as without them the navmesh may break.

        private readonly Chunk Chunk;
        private readonly bool[] alreadyMeshed = new bool[CubeMap.RegionSizeCubed];

        public NavMeshChunk(Vector3Int position, Chunk chunk)
        {
            Position = position;
            Chunk = chunk;
        }

        public void Init(NavMesh mesh)
        {
            mesh.NavChunks.TryGetValue(Position + CubeMap.RegionSize * Vector3Int.left, out ChunkNX);
            mesh.NavChunks.TryGetValue(Position + CubeMap.RegionSize * Vector3Int.back, out ChunkNZ);
            var yLimit = Chunk.HasAnyBlock() ? CubeMap.RegionSize : 1;
            for (var y = 0; y < yLimit; y++)
            {
                NavMeshPlanes[y] = new List<NavMeshPlane>();
            }
            for (var y = 0; y < yLimit; y++)
            {
                GeneratePlanes(y);
            }
        }

        public void EnsureConnectivity()
        {
            foreach(var kv in NavMeshPlanes)
            {
                kv.Value.ForEach(plane => plane.AddConnections(this));
            }
        }

        private void GeneratePlanes(int y)
        {
            NavMeshPlanes[y] ??= new List<NavMeshPlane>();
            var planes = NavMeshPlanes[y];
            var map = GlobalSettings.Instance.Map;
            var topped = y + 1 == CubeMap.RegionSize;
            var bottomed = y == 0;
            var maxH = GlobalSettings.Instance.Map.H - 1;
            var index = 0;
            for (var z = 0; z < CubeMap.RegionSize; z++)
            {
                for (var x = 0; x < CubeMap.RegionSize; x++)
                {
                    var pos = Position + new Vector3Int(x, y, z);
                        var alreadyIndex = (((z << CubeMap.RegionSizeShift) + y) << CubeMap.RegionSizeShift) + x;
                    if (!alreadyMeshed[alreadyIndex]) 
                    {
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
                            var plane = new NavMeshPlane(this);
                            TryGreedyMesh(plane, x, y, z);
                            plane.BlockMesh(alreadyMeshed);
                            plane.AddConnections(this);
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
                if (!NavMeshPlanes.TryGetValue(pY + y, out var planes))
                {
                    planes = new List<NavMeshPlane>();
                    NavMeshPlanes[pY + y] = planes;
                }
                for (var i = planes.Count - 1; i >= 0; i--)
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
                            plane.UnblockMesh(alreadyMeshed);
                            plane.RemoveConnections();
                        }
                    }
                }
            }
            
            if (pY > 0) GeneratePlanes(pY - 1);
            GeneratePlanes(pY);
            if (pY < sizeM1) GeneratePlanes(pY + 1);
        }

        private void TryGreedyMesh(NavMeshPlane plane, int sx, int y, int sz)
        {
            var map = GlobalSettings.Instance.Map;
            var topped = y + 1 == CubeMap.RegionSize;
            var bottomed = y == 0;
            var maxH = map.H - 1;
            plane.minX = (byte) sx;
            plane.minZ = (byte)sz;
            plane.Y = (byte)y;
            plane.maxZ = (byte)sz;
            for (var z = sz + 1; z < CubeMap.RegionSize; z++)
            {
                var p = Position + new Vector3Int(sx, y, z);
                var index = (((z << CubeMap.RegionSizeShift) + y) << CubeMap.RegionSizeShift) + sx;
                if (alreadyMeshed[index] || !(
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
                plane.maxZ = (byte) z;
            }

            plane.maxX = (byte) sx;
            for (var x = sx + 1; x < CubeMap.RegionSize; x++)
            {
                bool passed = true;
                for (var z = sz; z <= plane.maxZ; z++)
                {
                    var p = Position + new Vector3Int(x, y, z);
                    var index = (((z << CubeMap.RegionSizeShift) + y) << CubeMap.RegionSizeShift) + x;
                    if (alreadyMeshed[index] || !(
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
                    plane.maxX = (byte) x;
                }
                else break;
            }
        }

#if UNITY_EDITOR
        public static readonly Mesh QuadMesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");

        public void RenderGizmo(float opacity)
        {
            var color = Gizmos.color;
            var nowColor = new Color(0, 0.5f, 1, opacity);
            var lineColor = new Color(1, 0, 0, opacity);
            Gizmos.color = nowColor;
            foreach(var kv in NavMeshPlanes)
            {
                foreach(var plane in kv.Value)
                {
                    var scale = new Vector3(plane.maxX - plane.minX + 1, plane.maxZ - plane.minZ + 1, 1);
                    var p = Position + new Vector3Int(plane.minX, plane.Y, plane.minZ) + new Vector3(scale.x, scale.z, scale.y) / 2 + Vector3.down * 0.45f;
                    var rot = Quaternion.Euler(90, 0, 0);
                    Gizmos.DrawMesh(QuadMesh, p, rot, scale);
                    Gizmos.DrawWireMesh(QuadMesh, p, rot, scale);
                    Gizmos.color = lineColor;
                    foreach (var con in plane.Neighbours)
                    {
                        Gizmos.DrawLine(
                            plane.Chunk.Position + new Vector3(plane.minX + plane.maxX + 1, plane.Y * 2 + 0.1f, plane.minZ + plane.maxZ + 1) / 2,
                            con.Chunk.Position + new Vector3(con.minX + con.maxX + 1, con.Y * 2 + 0.1f, con.minZ + con.maxZ + 1) / 2
                        );
                    }
                    Gizmos.color = nowColor;
                }
            }
            Gizmos.color = color;
        }
#endif
    }
}
