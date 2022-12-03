using Assets.Scripts.ConfigScripts;
using Assets.Scripts.Village;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Jobs
{
    public abstract class Job
    {
        public abstract Vector3Int RepresentantPosition { get; }
        public abstract VillagerRole ViableRoles { get; }
        public abstract bool AllowLaddering { get; }
        public int Penalty { get; private set; }

        public abstract void Commit(Villager e);

        public void Penalize()
        {
            Penalty++;
        }

        protected static void TryMoveOrReturn(Villager e, List<Vector3Int> positions, Action then)
        {
            if (positions.Count == 0 || e.PathingTries > 5)
            {
                GlobalSettings.Instance.JobScheduler.ReturnJobOf(e);
            }
            else
            {
                if (positions.Contains(e.CurrentPosition))
                {
                    then();
                }
                else
                {
                    e.CurrentGoal = positions;
                }
            }
        }

        protected static void TryMoveOrElse(Villager e, List<Vector3Int> positions, Action then, Action @else)
        {
            if (positions.Count == 0 || e.PathingTries > 5)
            {
                @else();
            }
            else
            {
                if (positions.Contains(e.CurrentPosition))
                {
                    then();
                }
                else
                {
                    e.CurrentGoal = positions;
                }
            }
        }

        public abstract bool IsValid();

        public static readonly IReadOnlyDictionary<Type, Color> JobColorsDictionary = new Dictionary<Type, Color>
        {
            { typeof(MineBlockJob), new Color(240f / 255, 94f / 255, 35f / 255, 1) },
            { typeof(PlaceBlockJob), new Color(35f / 255, 240f / 255, 94f / 255, 1) },
            { typeof(HaulItemJob), new Color(1, 1, 1, 50f / 255) }
        };
    }
}
