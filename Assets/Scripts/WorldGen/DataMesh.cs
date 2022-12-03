using System.Collections.Generic;

namespace Assets.Scripts.WorldGen
{
    public struct DataMesh
    {
        public readonly List<ChunkVertex> vertexList;
        public readonly List<ushort> indexList;

        public DataMesh(List<ChunkVertex> vl, List<int> il)
        {
            vertexList = vl;
            indexList = il.ConvertAll(i => (ushort)i);
        }
    }
}