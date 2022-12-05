using System;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Entities
{
    public abstract class Entity
    {
        public readonly EntityTemplate Template;

        public abstract int MeshIndex { get; }

        protected Entity(EntityTemplate template)
        {
            Template = template;
        }

        public Vector3 Position;
        public Vector3Int CurrentPosition => Vector3Int.FloorToInt(Position);

        public abstract Vector3 Size { get; }
        public abstract byte[] GetEntityData();

        public byte[] Serialize()
        {
            var entityTypeData = Encoding.UTF8.GetBytes(GetType().Name);

            var eData = new byte[16];
            BitConverter.GetBytes(Position.x).CopyTo(eData, 0);
            BitConverter.GetBytes(Position.y).CopyTo(eData, 4);
            BitConverter.GetBytes(Position.z).CopyTo(eData, 8);

            var additionalData = GetEntityData();
            BitConverter.GetBytes(additionalData.Length).CopyTo(eData, 12);

            return BitConverter.GetBytes(entityTypeData.Length)
                    .Concat(entityTypeData)
                    .Concat(eData)
                    .Concat(additionalData)
                    .ToArray();
        }
    }

    public abstract class Entity<T, V> : Entity
        where T : Entity<T, V>
        where V : EntityTemplate<V, T>
    {
        public new readonly V Template;

        protected Entity(V template) : base(template)
        {
            Template = template;
        }
    }
}
