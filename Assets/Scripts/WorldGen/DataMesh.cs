using System.Collections.Generic;

namespace Assets.Scripts.WorldGen
{
    public struct DataMesh
    {
        public readonly List<ChunkVertex> vertexList;
        public readonly List<int> indexList;

        public DataMesh(List<ChunkVertex> vl, List<int> il)
        {
            vertexList = vl;
            indexList = il;
        }
    }
}