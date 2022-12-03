using Assets.Scripts.ConfigScripts;
using Assets.Scripts.Entities;
using Assets.Scripts.PathFinding;
using Assets.Scripts.WorldGen;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Items
{
    public class Item : Entity<Item, ItemTemplate>
    {
        public bool FallLock;
        public bool Falling { get; private set; }
        public Vector3Int FallPosition;

        public Item(ItemTemplate template) : base(template)
        {
        }

        public void TryFall(float deltaTime, float timeScale)
        {
            if (FallLock)
            {
                Falling = false;
            }
            else
            {
                var gravity = GlobalSettings.Variables["Gravity"].AsInt();
                if (Falling)
                {
                    Position = Vector3.MoveTowards(Position, FallPosition, gravity * deltaTime);
                }
                if ((CurrentPosition - Position).sqrMagnitude < 0.001f * timeScale)
                {
                    var bottomBlock = CurrentPosition.GetBlockAtFace(BlockDirection.BOTTOM);
                    Falling = BlockPathFinding.IsWalkable(bottomBlock);
                    if (Falling)
                    {
                        FallPosition = bottomBlock;
                        Position = Vector3.MoveTowards(Position, FallPosition, gravity * deltaTime);
                    }
                }
            }
        }

        public override Vector3 Size => Vector3.one;

        public override int MeshIndex => 0;

        public bool Visible = true;

        public override byte[] GetEntityData()
        {
            return Encoding.UTF8.GetBytes(Template.name);
        }
    }
}