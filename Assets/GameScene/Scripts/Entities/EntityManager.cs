using Assets.Scripts.Entities;
using Assets.Scripts.Entities.Templates;
using Assets.Scripts.Utilities;
using Assets.Scripts.Utilities.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Items
{
    [CreateAssetMenu(fileName = "EntityManagerDB", menuName = "EntityManager", order = 2)]
    public class EntityManager : ScriptableObject
    {
        public CrateTemplate CrateTemplate;

        public Dictionary<Type, Dictionary<Mesh, List<Entity>>> Entities;

        private readonly Dictionary<Mesh, InstanceBatcher> EntityBatching;

        public List<Type> UpdatableBlockTypes;

        public EntityManager()
        {
            Entities = new Dictionary<Type, Dictionary<Mesh, List<Entity>>>();
            EntityBatching = new Dictionary<Mesh, InstanceBatcher>();
            UpdatableBlockTypes = new List<Type>();
        }

        public void AddNewEntity(Entity e, bool update = true)
        {
            var mesh = e.Template.Meshes[e.MeshIndex];
            var material = e.Template.Materials;
            bool newThing = false;
            if (!Entities.TryGetValue(e.GetType(), out var typeDictionary))
            {
                typeDictionary = new Dictionary<Mesh, List<Entity>>();
                newThing = true;
                Entities[e.GetType()] = typeDictionary;
            }
            if (!typeDictionary.TryGetValue(mesh, out var entityList))
            {
                entityList = new List<Entity>();
                newThing = true;
                typeDictionary[mesh] = entityList;
            }
            entityList.Add(e);
            if (update || newThing)
            {
                UpdateBatch(mesh, material, entityList);
                var curPos = e.CurrentPosition;
                var reAddList = new List<Entity>();
                var batchList = new List<KeyValuePair<Mesh, List<Entity>>>();
                foreach (var t in UpdatableBlockTypes)
                {
                    foreach (var m in Entities[t])
                    {
                        bool batch = false;
                        for (int i = m.Value.Count - 1; i >= 0; i--)
                        {
                            if ((m.Value[i].CurrentPosition - curPos).sqrMagnitude < 2)
                            {
                                reAddList.Add(m.Value[i]);
                                m.Value.RemoveAt(i);
                                batch = true;
                            }
                        }
                        if (batch) batchList.Add(m);
                    }
                }
                for (int i = 0; i < reAddList.Count; i++)
                {
                    AddNewEntity(reAddList[i], false);
                }
                foreach(var b in batchList)
                {
                    UpdateBatch(b.Key, null, b.Value);
                }
            }
        }

        private void UpdateBatch(Mesh mesh, Material[] materials, List<Entity> list)
        {
            var halfVector = (Vector3.one * 0.5f)._x0z();
            var matrixList = list.Select(i => Matrix4x4.Translate(i.Position + halfVector)).ToList();
            if (!EntityBatching.TryGetValue(mesh, out var batcher))
            {
                batcher = new InstanceBatcher
                {
                    Mesh = mesh,
                    Materials = materials
                };
                EntityBatching[mesh] = batcher;
            }

            batcher.Rebatch(matrixList);
        }

        public void RenderEntities()
        {
            var layer = LayerMask.NameToLayer("Items");
            foreach(var kv in EntityBatching)
            {
                kv.Value.Render(layer);
            }
        }
    }
}
