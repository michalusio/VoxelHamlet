using Assets.Scripts.ConfigScripts;
using Assets.Scripts.Items;
using Assets.Scripts.PathFinding;
using Assets.Scripts.Village;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Jobs
{
    public class HaulItemJob : Job
    {
        public Item Item { get; }

        public override Vector3Int RepresentantPosition => Item.CurrentPosition;
        public override VillagerRole ViableRoles => VillagerRole.Hauler;

        public override bool AllowLaddering => false;

        private List<Vector3Int> jobPositions;

        public HaulItemJob(Item item)
        {
            Item = item;
            jobPositions = PickupJobPositions.Select(p => p + RepresentantPosition).Where(p => GlobalSettings.Instance.Map.IsInBounds(p)).ToList();
            RefillItemPosition = true;
        }

        //State
        private bool RefillItemPosition;
        private bool PickedUp, WaitForPickup;
        private (Storage, Vector3Int?) storageSpace;
        //State

        public override void Commit(Villager e)
        {
            if (Item.Falling)
            {
                RefillItemPosition = true;
                GlobalSettings.Instance.JobScheduler.ReturnJobOf(e);
                return;
            }
            if (RefillItemPosition)
            {
                jobPositions = PickupJobPositions.Select(p => p + RepresentantPosition).Where(p => GlobalSettings.Instance.Map.IsInBounds(p)).ToList();
                RefillItemPosition = false;
            }
            if (storageSpace.Item1 == null)
            {
                storageSpace = GlobalSettings.Instance.JobScheduler
                    .Storages
                    .Select(s => (s, s.GetFreeSpot()))
                    .FirstOrDefault(s => s.Item2.HasValue);
                if (storageSpace.Item1 != null)
                {
                    storageSpace.Item1.LockItemSpot(storageSpace.Item2.Value);
                }
                else
                {
                    GlobalSettings.Instance.JobScheduler.ReturnJobOf(e);
                    return;
                }
            }
            if (WaitForPickup)
            {
                if (e.FreeToWork)
                {
                    WaitForPickup = false;
                    PickedUp = true;
                }
            }
            else
            {
                if (PickedUp)
                {
                    Item.Position = e.RightHandTransform.position;
                }
                var validPositions = jobPositions.Where(p => BlockPathFinding.IsValidForEntity(p)).ToList();
                TryMoveOrElse(e, validPositions, () =>
                {
                    if (!PickedUp)
                    {
                        WaitForPickup = true;
                        jobPositions = PickupJobPositions
                                        .Select(p => p + storageSpace.Item2.Value)
                                        .Where(p => GlobalSettings.Instance.Map.IsInBounds(p))
                                        .ToList();
                        VillagerActions.PickUpItem(e, Item);
                    }
                    else
                    {
                        VillagerActions.PutDownItem(e, Item, storageSpace.Item2.Value, () =>
                        {
                            Item.Position = storageSpace.Item2.Value;
                            storageSpace.Item1.AddItemToSpot(storageSpace.Item2.Value, Item);
                        });
                        GlobalSettings.Instance.JobScheduler.JobDone(e);
                    }
                }, () =>
                {
                    if (PickedUp)
                    {
                        var storage = storageSpace.Item1;
                        var spot = storageSpace.Item2;

                        VillagerActions.PutDownItem(e, Item, e.CurrentPosition + new Vector3Int(1, 0, 1), () =>
                        {
                            storage?.UnlockItemSpot(spot.Value);
                            Item.Position = e.CurrentPosition;
                        });

                        jobPositions = PickupJobPositions.Select(p => p + e.CurrentPosition).Where(p => GlobalSettings.Instance.Map.IsInBounds(p)).ToList();
                        PickedUp = false;
                        RefillItemPosition = true;
                        storageSpace = default;
                        GlobalSettings.Instance.JobScheduler.ReturnJobOf(e);
                    }
                    else
                    {
                        WaitForPickup = false;
                        storageSpace.Item1?.UnlockItemSpot(storageSpace.Item2.Value);
                        PickedUp = false;
                        RefillItemPosition = true;
                        storageSpace = default;
                        GlobalSettings.Instance.JobScheduler.ReturnJobOf(e);
                    }
                });
            }
        }

        public override bool IsValid()
        {
            return Item != null;
        }

        private static readonly List<Vector3Int> PickupJobPositions = new List<Vector3Int>
        {
            new Vector3Int(1, 1, -1),
            new Vector3Int(-2, 1, -1),
            new Vector3Int(-1, 1, 1),
            new Vector3Int(-1, 1, -2),
            new Vector3Int(1, 1, 0),
            new Vector3Int(-2, 1, 0),
            new Vector3Int(0, 1, 1),
            new Vector3Int(0, 1, -2),

            new Vector3Int(0, 1, 0),
            new Vector3Int(-1, 1, 0),
            new Vector3Int(0, 1, -1),
            new Vector3Int(-1, 1, -1),


            new Vector3Int(0, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 0, -1),
            new Vector3Int(-1, 0, -1),

            new Vector3Int(1, 0, -1),
            new Vector3Int(-2, 0, -1),
            new Vector3Int(-1, 0, 1),
            new Vector3Int(-1, 0, -2),
            new Vector3Int(1, 0, 0),
            new Vector3Int(-2, 0, 0),
            new Vector3Int(0, 0, 1),
            new Vector3Int(0, 0, -2),


            new Vector3Int(1, -1, -1),
            new Vector3Int(-2, -1, -1),
            new Vector3Int(-1, -1, 1),
            new Vector3Int(-1, -1, -2),
            new Vector3Int(1, -1, 0),
            new Vector3Int(-2, -1, 0),
            new Vector3Int(0, -1, 1),
            new Vector3Int(0, -1, -2),
        };
    }
}
