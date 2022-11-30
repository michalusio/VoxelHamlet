using Assets.Scripts.Entities;
using Assets.Scripts.Entities.Objects;
using Assets.Scripts.Items;
using Assets.Scripts.Utilities;
using UnityEngine;

namespace Assets.Scripts.Village
{
    public class Storage
    {
        public readonly Rect3Int Area;
        public readonly Entity[,] Items;
        public readonly bool[,] Locks;

        public Storage(Vector3Int min, Vector3Int max)
        {
            Area = new Rect3Int(min, max);
            Items = new Entity[Area.Size.x, Area.Size.z];
            Locks = new bool[Area.Size.x, Area.Size.z];
            var c = new Crate(GlobalSettings.Instance.EntityManager.CrateTemplate)
            {
                Position = Area.Min
            };
            Items[0, 0] = c;
            GlobalSettings.Instance.EntityManager.AddNewEntity(c);
        }

        public Vector3Int? GetFreeSpot()
        {
            for (int x = 0; x < Items.GetLength(0); x++)
            {
                for (int z = 0; z < Items.GetLength(1); z++)
                {
                    if ((Items[x, z] == null && !Locks[x, z]) || (Items[x, z] is Crate c && c.ItemsInside.Count + c.PlusLocks < c.Template.CrateMaxItemCount))
                        return Area.Min + new Vector3Int(x, 0, z);
                }
            }
            return null;
        }

        public void LockItemSpot(Vector3Int spot)
        {
            var vsArea = spot - Area.Min;
            if (Items[vsArea.x, vsArea.z] is Crate c)
            {
                c.LockPlace();
            }
            else Locks[vsArea.x, vsArea.z] = true;
        }

        public void UnlockItemSpot(Vector3Int spot)
        {
            var vsArea = spot - Area.Min;
            if (Items[vsArea.x, vsArea.z] is Crate c)
            {
                c.UnlockPlace();
            }
            else Locks[vsArea.x, vsArea.z] = false;
        }

        internal void AddItemToSpot(Vector3Int spot, Item item)
        {
            var vsArea = spot - Area.Min;
            if (Items[vsArea.x, vsArea.z] is Crate c)
            {
                c.AddItem(item);
            }
            else
            {
                Items[vsArea.x, vsArea.z] = item;
                Locks[vsArea.x, vsArea.z] = false;
            }
        }
    }
}