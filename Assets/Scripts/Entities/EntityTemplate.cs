using UnityEngine;

namespace Assets.Scripts.Entities
{
    public class EntityTemplate : ScriptableObject
    {
        public Mesh[] Meshes;
        public Material[] Materials;
    }
    public class EntityTemplate<T, V> : EntityTemplate
        where T : EntityTemplate<T, V>
        where V : Entity<V, T>
    {
        
    }
}
