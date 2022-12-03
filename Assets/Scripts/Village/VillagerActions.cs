using Assets.Scripts.ConfigScripts;
using Assets.Scripts.Items;
using Assets.Scripts.Jobs;
using Assets.Scripts.Utilities.Math;
using Assets.Scripts.WorldGen;
using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Village
{
    public static class VillagerActions
    {
        public static void PlaceBlock(Villager e, Vector3Int blockPosition, BlockType blockType)
        {
            e.FreeToWork = false;
            e.Animator.Play("Base Layer.MineStart");
            var newForward = ((Vector3)(blockPosition - e.CurrentPosition))._x0z().normalized;
            e.StartCoroutine(PlaceWaitCoroutine());

            IEnumerator PlaceWaitCoroutine()
            {
                var timeStart = Time.time;
                const float animationTime = 0.5f;
                while ((Time.time - timeStart) < animationTime)
                {
                    e.transform.forward = Vector3.RotateTowards(e.transform.forward, newForward, 2 * Mathf.Deg2Rad / animationTime, 0.01f);
                    yield return null;
                }
                var b = GlobalSettings.Instance.Map[blockPosition];
                b.BlockType = blockType;
                GlobalSettings.Instance.Map[blockPosition] = b;
                e.FreeToWork = true;
            }
        }

        public static void MineBlock(Villager e, Vector3Int blockPosition)
        {
            e.FreeToWork = false;
            e.Animator.Play("Base Layer.MineStart");
            var newForward = ((Vector3)(blockPosition - e.CurrentPosition))._x0z().normalized;
            e.StartCoroutine(MiningWaitCoroutine());

            IEnumerator MiningWaitCoroutine()
            {
                var timeStart = Time.time;
                const float animationTime = 0.5f;
                while ((Time.time - timeStart) < animationTime)
                {
                    e.transform.forward = Vector3.RotateTowards(e.transform.forward, newForward, 2 * Mathf.Deg2Rad / animationTime, 0.01f);
                    yield return null;
                }
                var newItem = GlobalSettings.Instance.ItemManager.CreateItemForBlock(GlobalSettings.Instance.Map[blockPosition].BlockType);
                if (newItem != null)
                {
                    newItem.Position = blockPosition;
                    GlobalSettings.Instance.JobScheduler.AddJob(new HaulItemJob(newItem));
                }
                GlobalSettings.Instance.Map[blockPosition] = default;
                e.FreeToWork = true;
            }
        }

        public static void PickUpItem(Villager e, Item item)
        {
            item.FallLock = true;
            var spot = item.Position;
            e.FreeToWork = false;
            e.Animator.Play("Base Layer.PickUp");
            e.Animator.SetBool("HasItem", true);
            var newForward = (spot - e.CurrentPosition)._x0z().normalized;
            e.StartCoroutine(PickUpWaitCoroutine());

            IEnumerator PickUpWaitCoroutine()
            {
                var timeStart = Time.time;
                const float animationTime = 1f;
                while ((Time.time - timeStart) < animationTime)
                {
                    e.transform.forward = Vector3.RotateTowards(e.transform.forward, newForward, 2 * Mathf.Deg2Rad / animationTime, 0.01f);
                    if ((Time.time - timeStart) > animationTime * 0.5f)
                    {
                        item.Position = e.RightHandTransform.position;
                    }
                    yield return null;
                }
                e.FreeToWork = true;
            }
        }

        public static void PutDownItem(Villager e, Item item, Vector3Int spot, Action then = null)
        {
            e.FreeToWork = false;
            e.Animator.Play("Base Layer.PutDown");
            e.Animator.SetBool("HasItem", false);
            var newForward = ((Vector3)(spot - e.CurrentPosition))._x0z().normalized;
            e.StartCoroutine(PutDownWaitCoroutine());

            IEnumerator PutDownWaitCoroutine()
            {
                var timeStart = Time.time;
                const float animationTime = 1f;
                while ((Time.time - timeStart) < animationTime)
                {
                    e.transform.forward = Vector3.RotateTowards(e.transform.forward, newForward, 2 * Mathf.Deg2Rad / animationTime, 0.01f);
                    if ((Time.time - timeStart) < animationTime * 0.5f)
                    {
                        item.Position = e.RightHandTransform.position;
                    }
                    yield return null;
                }
                then?.Invoke();
                item.FallLock = false;
                e.FreeToWork = true;
            }
        }
    }
}
