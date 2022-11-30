using Assets.Scripts.Entities;

namespace Assets.Scripts.WorldGen
{
    public struct Block
    {
        public Entity EntityInBlock;
        public BlockType BlockType;

        public static bool operator ==(Block a, Block b)
        {
            return a.BlockType == b.BlockType && a.EntityInBlock == b.EntityInBlock;
        }

        public static bool operator !=(Block a, Block b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is Block b && this == b;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return BlockType.GetHashCode() * 13 + EntityInBlock.GetHashCode();
            }
        }
    }
}