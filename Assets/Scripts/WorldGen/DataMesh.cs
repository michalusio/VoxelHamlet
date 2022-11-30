using System.Collections.Generic;

namespace Assets.Scripts.WorldGen
{
    public struct DataMesh
    {
        public readonly List<ChunkVertex> vertexListPX, vertexListNX, vertexListPY, vertexListNY, vertexListPZ, vertexListNZ;
        public readonly List<int> indexListPX, indexListNX, indexListPY, indexListNY, indexListPZ, indexListNZ;

        public DataMesh(List<ChunkVertex> vlPX, List<ChunkVertex> vlNX,
                        List<ChunkVertex> vlPY, List<ChunkVertex> vlNY,
                        List<ChunkVertex> vlPZ, List<ChunkVertex> vlNZ,
                        List<int> ilPX, List<int> ilNX,
                        List<int> ilPY, List<int> ilNY,
                        List<int> ilPZ, List<int> ilNZ)
        {
            vertexListPX = vlPX;
            vertexListNX = vlNX;

            vertexListPY = vlPY;
            vertexListNY = vlNY;

            vertexListPZ = vlPZ;
            vertexListNZ = vlNZ;

            indexListPX = ilPX;
            indexListNX = ilNX;

            indexListPY = ilPY;
            indexListNY = ilNY;

            indexListPZ = ilPZ;
            indexListNZ = ilNZ;
        }
    }
}