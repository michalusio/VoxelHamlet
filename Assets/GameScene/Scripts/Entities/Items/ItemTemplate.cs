using Assets.Scripts.Entities;
using UnityEngine;

namespace Assets.Scripts.Items
{
    [CreateAssetMenu(fileName = "ItemTemplate", menuName = "ItemTemplate", order = 3)]
    public class ItemTemplate : EntityTemplate<ItemTemplate, Item>
    {
    }
}
