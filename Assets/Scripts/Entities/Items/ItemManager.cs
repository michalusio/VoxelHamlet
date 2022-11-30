using Assets.Scripts.Utilities;
using Assets.Scripts.WorldGen;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Items
{
    [CreateAssetMenu(fileName = "ItemManagerDB", menuName = "ItemManager", order = 2)]
    public class ItemManager : ScriptableObject
    {
        public List<ItemTemplate> Templates;

        private readonly Dictionary<ItemTemplate, (List<Item>, List<Matrix4x4>, InstanceBatcher)> Items;

        public ItemManager()
        {
            Items = new Dictionary<ItemTemplate, (List<Item>, List<Matrix4x4>, InstanceBatcher)>();
        }

        public Item CreateItemForBlock(BlockType type)
        {
            var halfVector = (Vector3.one * 0.5f)._x0z();
            ItemTemplate template = Templates.Find(t => t.name == type.ToString());
            if (template == null) return null;
            var item = new Item(template);
            if (!Items.TryGetValue(template, out var val))
            {
                val = (new List<Item>(), new List<Matrix4x4>(), new InstanceBatcher
                {
                    Materials = template.Materials,
                    Mesh = template.Meshes[0]
                });
                Items[template] = val;
            }
            val.Item1.Add(item);
            val.Item2.Add(Matrix4x4.Translate(item.Position + halfVector));
            return item;
        }

        public void Update()
        {
            var deltaTime = Time.deltaTime;
            var timeScale = Time.timeScale;
            foreach(var kv in Items)
            {
                foreach (var item in kv.Value.Item1)
                {
                    item.TryFall(deltaTime, timeScale);
                }
            }
        }

        public void RenderItems()
        {
            {
                var halfVector = (Vector3.one * 0.5f)._x0z();
                foreach(var kv in Items)
                {
                    var itemList = kv.Value.Item1;
                    var matrixList = kv.Value.Item2;
                    matrixList.Clear();

                    for (int i = 0; i < itemList.Count; i++)
                    {
                        if (itemList[i].Visible)
                        {
                            matrixList.Add(Matrix4x4.Translate(itemList[i].Position + halfVector));
                        }
                    }
                    kv.Value.Item3.Rebatch(matrixList);
                }
            }
            var layer = LayerMask.NameToLayer("Items");
            foreach(var kv in Items)
            {
                kv.Value.Item3.Render(layer);
            }
        }
    }
}
