using Assets.Scripts.Entities.Templates;
using UnityEngine;

namespace Assets.Scripts.Entities.Objects
{
    public class Fence : Entity<Fence, FenceTemplate>
    {
        public Fence(FenceTemplate template) : base(template)
        {
        }

        public override Vector3 Size => Vector3.one;

        private int _meshIndex;
        public override int MeshIndex => _meshIndex;

        public override byte[] GetEntityData()
        {
            return System.Array.Empty<byte>();
        }
    }
}
