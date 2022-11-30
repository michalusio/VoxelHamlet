using UnityEngine;

namespace Assets.Scripts.WorldGen.GenSteps
{
    public class PlaceTerrainDetails : IGeneratorStep
    {
        private readonly int Tries;

        public PlaceTerrainDetails(int tries)
        {
            Tries = tries;
        }

        public void Commit(CubeMap map)
        {
            foreach(var kv in map.GetChunks)
            {
                for(int i = 0; i < Tries; i++)
                {
                    var randomX = Random.Range(0, map.W);
                    var randomZ = Random.Range(0, map.D);
                    var randomPos = new Vector3Int(randomX, map.GetHighestYAt(randomX, randomZ), randomZ);
                    if (map[randomPos].BlockType == BlockType.Grass)
                    {
                        var topPos = randomPos.GetBlockAtFace(BlockDirection.TOP);

                        var leftPos = topPos.GetBlockAtFace(BlockDirection.NegX);

                        var rightPos = topPos.GetBlockAtFace(BlockDirection.PosX);

                        var backPos = topPos.GetBlockAtFace(BlockDirection.NegZ);

                        var frontPos = topPos.GetBlockAtFace(BlockDirection.PosZ);

                        if ((map.IsInBounds(leftPos) && map[leftPos].BlockType == BlockType.Stone) ||
                            (map.IsInBounds(rightPos) && map[rightPos].BlockType == BlockType.Stone))
                        {
                            map.SetCube(randomPos.GetBlockAtFace(BlockDirection.NegZ), new Vector3Int(1, 3, 2), (b, v) => BlockType.Stone);
                        }
                        if ((map.IsInBounds(backPos) && map[backPos].BlockType == BlockType.Stone) ||
                            (map.IsInBounds(frontPos) && map[frontPos].BlockType == BlockType.Stone))
                        {
                            map.SetCube(randomPos.GetBlockAtFace(BlockDirection.NegX), new Vector3Int(2, 3, 1), (b, v) => BlockType.Stone);
                        }

                        if ((map.IsInBounds(leftPos) && map[leftPos].BlockType == BlockType.Dirt) ||
                            (map.IsInBounds(rightPos) && map[rightPos].BlockType == BlockType.Dirt))
                        {
                            map.SetCube(randomPos.GetBlockAtFace(BlockDirection.NegZ), new Vector3Int(1, 3, 2), (b, v) => BlockType.Dirt);
                        }
                        if ((map.IsInBounds(backPos) && map[backPos].BlockType == BlockType.Dirt) ||
                            (map.IsInBounds(frontPos) && map[frontPos].BlockType == BlockType.Dirt))
                        {
                            map.SetCube(randomPos.GetBlockAtFace(BlockDirection.NegX), new Vector3Int(2, 3, 1), (b, v) => BlockType.Dirt);
                        }
                    }
                }
            }
        }
    }
}
