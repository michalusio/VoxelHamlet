using Assets.Scripts.Entities.Templates;
using Assets.Scripts.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Entities.Objects
{
    public class Crate : Entity<Crate, CrateTemplate>
    {
        public override Vector3 Size => Vector3.one * 2;

        public override int MeshIndex => 0;

        public readonly List<(Item, bool)> ItemsInside;
        public int PlusLocks { get; private set; }

        public Crate(CrateTemplate template) : base(template)
        {
            ItemsInside = new List<(Item, bool)>();
        }

        public void LockPlace()
        {
            PlusLocks++;
        }

        public void UnlockPlace()
        {
            PlusLocks--;
        }

        public void AddItem(Item item)
        {
            ItemsInside.Add((item, false));
            item.Visible = false;
            UnlockPlace();
        }

        public override byte[] GetEntityData()
        {
            return BitConverter.GetBytes(PlusLocks)
                .Concat(ItemsInside.SelectMany(i =>
                    {
                        var itemData = i.Item1.Serialize();
                        return BitConverter.GetBytes(itemData.Length+1).Concat(new byte[] { (byte) (i.Item2 ? 1 : 0) }).Concat(itemData);
                    }))
                .ToArray();
        }
    }
}
