namespace Assets.Scripts.WorldGen
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct ChunkVertex
    {
        public byte x;
        public byte y;
        public byte z;
        public byte blockIndex;//4

        public override bool Equals(object obj)
        {
            return obj is ChunkVertex cv && this == cv;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((x.GetHashCode() * 17 + y.GetHashCode()) * 17 + z.GetHashCode()) * 17 + blockIndex.GetHashCode();
            }
        }

        public static bool operator ==(ChunkVertex left, ChunkVertex right)
        {
            return left.x == right.x && left.y == right.y && left.z == right.z && left.blockIndex == right.blockIndex;
        }

        public static bool operator !=(ChunkVertex left, ChunkVertex right)
        {
            return !(left == right);
        }
    }
}