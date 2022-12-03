using Assets.Scripts.ConfigScripts;
using Assets.Scripts.UI;
using Assets.Scripts.WorldGen;
using UnityEngine;

namespace Assets.Scripts.Utilities
{
    public static class BlockPlaceFunctions
    {
        public static void PlaceStair(CubeMap editMap, Block b, Vector3Int start, Vector3Int end, bool full, int width)
        {
            if (end.y < start.y)
            {
                (start, end) = (end, start);
            }
            var dx = Mathf.Abs(end.x - start.x);
            var dz = Mathf.Abs(end.z - start.z);
            var dy = end.y - start.y;

            var xSign = System.Math.Sign(end.x - start.x);
            var zSign = System.Math.Sign(end.z - start.z);

            var dxdz = dx > dz;
            var chosenLength = dxdz ? dx : dz;
            if (chosenLength < dy) return;

            width--;
            if (dxdz) start.z -= width;
            else start.x -= width;
            for (int w = -width; w <= width; w++)
            {
                for (int i = 0; i < dy; i++)
                {
                    if (full)
                    {
                        var delta = new Vector3Int(dxdz ? i * xSign : 0, 0, dxdz ? 0 : i * zSign);
                        for (int y = 0; y <= i; y++)
                        {
                            delta.y = y;
                            SetBlock(editMap, start + delta, b);
                        }
                    }
                    else
                    {
                        var delta = new Vector3Int(dxdz ? i * xSign : 0, i, dxdz ? 0 : i * zSign);
                        SetBlock(editMap, start + delta, b);
                        if (dxdz) delta.x += xSign;
                        else delta.z += zSign;
                        SetBlock(editMap, start + delta, b);
                    }
                }
                for (int i = dy; i <= chosenLength; i++)
                {
                    var delta = new Vector3Int(dxdz ? i * xSign : 0, dy, dxdz ? 0 : i * zSign);
                    if (full)
                    {
                        for (int y = 0; y <= dy; y++)
                        {
                            delta.y = y;
                            SetBlock(editMap, start + delta, b);
                        }
                    } else
                    {
                        SetBlock(editMap, start + delta, b);
                    }
                }
                if (dxdz) start.z++;
                else start.x++;
            }
        }

        public static void PlaceCube(CubeMap editMap, Block b, Vector3Int start, Vector3Int end)
        {
            for (int x = Mathf.Min(start.x, end.x); x <= Mathf.Max(start.x, end.x); x++)
            {
                for (int y = Mathf.Min(start.y, end.y); y <= Mathf.Max(start.y, end.y); y++)
                {
                    for (int z = Mathf.Min(start.z, end.z); z <= Mathf.Max(start.z, end.z); z++)
                    {
                        SetBlock(editMap, new Vector3Int(x, y, z), b);
                    }
                }
            }
        }

        public static void PlaceRoom(CubeMap editMap, Block b, Vector3Int start, Vector3Int end, int height)
        {
            PlaceWall(editMap, b, new Vector3Int(start.x, start.y, start.z), new Vector3Int(end.x, start.y, start.z), height);
            PlaceWall(editMap, b, new Vector3Int(start.x, start.y, start.z), new Vector3Int(start.x, start.y, end.z), height);
            PlaceWall(editMap, b, new Vector3Int(end.x, start.y, start.z), new Vector3Int(end.x, start.y, end.z), height);
            PlaceWall(editMap, b, new Vector3Int(start.x, start.y, end.z), new Vector3Int(end.x, start.y, end.z), height);
        }

        public static void PlacePlane(CubeMap editMap, Block b, Vector3Int start, Vector3Int end)
        {
            var dx = Mathf.Abs(start.x - end.x);
            var dy = Mathf.Abs(start.y - end.y);
            var dz = Mathf.Abs(start.z - end.z);
            if (dy <= dx && dy <= dz)
            {
                for (int x = Mathf.Min(start.x, end.x); x <= Mathf.Max(start.x, end.x); x++)
                {
                    for (int z = Mathf.Min(start.z, end.z); z <= Mathf.Max(start.z, end.z); z++)
                    {
                        SetBlock(editMap, new Vector3Int(x, start.y, z), b);
                    }
                }
            }
            else if (dz <= dx && dz <= dy)
            {
                for (int x = Mathf.Min(start.x, end.x); x <= Mathf.Max(start.x, end.x); x++)
                {
                    for (int y = Mathf.Min(start.y, end.y); y <= Mathf.Max(start.y, end.y); y++)
                    {
                        SetBlock(editMap, new Vector3Int(x, y, start.z), b);
                    }
                }
            }
            else
            {
                for (int y = Mathf.Min(start.y, end.y); y <= Mathf.Max(start.y, end.y); y++)
                {
                    for (int z = Mathf.Min(start.z, end.z); z <= Mathf.Max(start.z, end.z); z++)
                    {
                        SetBlock(editMap, new Vector3Int(start.x, y, z), b);
                    }
                }
            }
        }

        public static void PlaceFrame(CubeMap editMap, Block b, Vector3Int start, Vector3Int end)
        {
            var dx = Mathf.Abs(start.x - end.x);
            var dy = Mathf.Abs(start.y - end.y);
            var dz = Mathf.Abs(start.z - end.z);
            var minx = Mathf.Min(start.x, end.x);
            var maxx = Mathf.Max(start.x, end.x);
            var miny = Mathf.Min(start.y, end.y);
            var maxy = Mathf.Max(start.y, end.y);
            var minz = Mathf.Min(start.z, end.z);
            var maxz = Mathf.Max(start.z, end.z);
            if (dy <= dx && dy <= dz)
            {
                for (int x = Mathf.Min(start.x, end.x); x <= Mathf.Max(start.x, end.x); x++)
                {
                    for (int z = Mathf.Min(start.z, end.z); z <= Mathf.Max(start.z, end.z); z++)
                    {
                        if (x == minx || x == maxx || z == minz || z == maxz)
                        {
                            SetBlock(editMap, new Vector3Int(x, start.y, z), b);
                        }
                    }
                }
            }
            else if (dz <= dx && dz <= dy)
            {
                for (int x = Mathf.Min(start.x, end.x); x <= Mathf.Max(start.x, end.x); x++)
                {
                    for (int y = Mathf.Min(start.y, end.y); y <= Mathf.Max(start.y, end.y); y++)
                    {
                        if (x == minx || x == maxx || y == miny || y == maxy)
                        {
                            SetBlock(editMap, new Vector3Int(x, y, start.z), b);
                        }
                    }
                }
            }
            else
            {
                for (int y = Mathf.Min(start.y, end.y); y <= Mathf.Max(start.y, end.y); y++)
                {
                    for (int z = Mathf.Min(start.z, end.z); z <= Mathf.Max(start.z, end.z); z++)
                    {
                        if (y == miny || y == maxy || z == minz || z == maxz)
                        {
                            SetBlock(editMap, new Vector3Int(start.x, y, z), b);
                        }
                    }
                }
            }
        }

        public static void PlaceLine(CubeMap editMap, Block b, Vector3Int start, Vector3Int end)
        {
            var dx = Mathf.Abs(start.x - end.x);
            var dy = Mathf.Abs(start.y - end.y);
            var dz = Mathf.Abs(start.z - end.z);
            if (dx >= dy && dx >= dz)
            {
                for (int i = Mathf.Min(start.x, end.x); i <= Mathf.Max(start.x, end.x); i++)
                {
                    SetBlock(editMap, new Vector3Int(i, start.y, start.z), b);
                }
            }
            else if (dy > dz)
            {
                for (int i = Mathf.Min(start.y, end.y); i <= Mathf.Max(start.y, end.y); i++)
                {
                    SetBlock(editMap, new Vector3Int(start.x, i, start.z), b);
                }
            }
            else
            {
                for (int i = Mathf.Min(start.z, end.z); i <= Mathf.Max(start.z, end.z); i++)
                {
                    SetBlock(editMap, new Vector3Int(start.x, start.y, i), b);
                }
            }
        }

        public static void PlaceWall(CubeMap editMap, Block b, Vector3Int start, Vector3Int end, int height)
        {
            var dx = Mathf.Abs(start.x - end.x);
            var dz = Mathf.Abs(start.z - end.z);
            if (dx >= dz)
            {
                for (int i = Mathf.Min(start.x, end.x); i <= Mathf.Max(start.x, end.x); i++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        SetBlock(editMap, new Vector3Int(i, start.y + y, start.z), b);
                    }
                }
            }
            else
            {
                for (int i = Mathf.Min(start.z, end.z); i <= Mathf.Max(start.z, end.z); i++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        SetBlock(editMap, new Vector3Int(start.x, start.y + y, i), b);
                    }
                }
            }
        }

        public static void PlaceRoof(CubeMap editMap, Block b, Vector3Int start, Vector3Int end, BlockHandle.HandleType roofHandling, float roofHeightMultiplier)
        {
            var minx = Mathf.Min(start.x, end.x);
            var maxx = Mathf.Max(start.x, end.x);
            var minz = Mathf.Min(start.z, end.z);
            var maxz = Mathf.Max(start.z, end.z);
            roofHandling &= BlockHandle.HandleType.All ^ BlockHandle.HandleType.NegY ^ BlockHandle.HandleType.Y;
            for (int x = Mathf.Min(start.x, end.x); x <= Mathf.Max(start.x, end.x); x++)
            {
                for (int z = Mathf.Min(start.z, end.z); z <= Mathf.Max(start.z, end.z); z++)
                {
                    var deltaY = 999;
                    if (roofHandling == 0) deltaY = 0;
                    if (roofHandling.HasFlag(BlockHandle.HandleType.X)) deltaY = Mathf.Min(maxx - x, deltaY);
                    if (roofHandling.HasFlag(BlockHandle.HandleType.NegX)) deltaY = Mathf.Min(x - minx, deltaY);
                    if (roofHandling.HasFlag(BlockHandle.HandleType.Z)) deltaY = Mathf.Min(maxz - z, deltaY);
                    if (roofHandling.HasFlag(BlockHandle.HandleType.NegZ)) deltaY = Mathf.Min(z - minz, deltaY);
                    SetBlock(editMap, new Vector3Int(x, start.y + Mathf.CeilToInt(deltaY * roofHeightMultiplier), z), b);
                }
            }
        }

        public static void SetBlock(CubeMap editMap, Vector3Int pos, Block b)
        {
            var size = GlobalSettings.Instance.EditMode.EditSize;
            if (pos.x < size.x && pos.z < size.y && pos.y < 64 && pos.x >= 0 && pos.y >= 0 && pos.z >= 0)
            {
                editMap[pos] = b;
            }
        }
    }
}
