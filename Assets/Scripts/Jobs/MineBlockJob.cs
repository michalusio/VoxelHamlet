using Assets.Scripts.PathFinding;
using Assets.Scripts.Utilities;
using Assets.Scripts.Village;
using Assets.Scripts.WorldGen;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Jobs
{
    public class MineBlockJob : Job
    {
        public Vector3Int Position { get; }

        public override Vector3Int RepresentantPosition => Position;
        public override bool AllowLaddering => false;
        public override VillagerRole ViableRoles => VillagerRole.Miner;

        private readonly List<Vector3Int> jobPositions;
        private readonly List<Vector3Int> validPositions;

        public MineBlockJob(Vector3Int pos)
        {
            Position = pos;
            jobPositions = new List<Vector3Int>(MineJobPositions.Count);
            foreach(var p in MineJobPositions)
            {
                var newPos = p + pos;
                if (GlobalSettings.Instance.Map.IsInBounds(newPos))
                {
                    jobPositions.Add(newPos);
                }
            }
            validPositions = new List<Vector3Int>(MineJobPositions.Count);
        }

        public override void Commit(Villager e)
        {
            validPositions.Clear();
            foreach(var p in jobPositions)
            {
                if (BlockPathFinding.IsValidForEntity(p))
                {
                    validPositions.Add(p);
                }
            }
            TryMoveOrReturn(e, validPositions, () =>
            {
                VillagerActions.MineBlock(e, Position);
                GlobalSettings.Instance.JobScheduler.JobDone(e);
            });
        }

        public override bool IsValid()
        {
            return GlobalSettings.Instance.Map[Position].BlockType != BlockType.Air &&
                GlobalSettings.Instance.JobScheduler.Storages.All(s => !s.Area.Inside(Position + Vector3Int.up));
        }

        private static readonly List<Vector3Int> MineJobPositions = new List<Vector3Int>
        {
            new Vector3Int(-2, 2, -2),
            new Vector3Int(-2, 2, 1),
            new Vector3Int(1, 2, -2),
            new Vector3Int(1, 2, 1),

            new Vector3Int(-2, 1, -2),
            new Vector3Int(-2, 1, 1),
            new Vector3Int(1, 1, -2),
            new Vector3Int(1, 1, 1),

            new Vector3Int(-2, 0, -2),
            new Vector3Int(-2, 0, 1),
            new Vector3Int(1, 0, -2),
            new Vector3Int(1, 0, 1),

            new Vector3Int(-2, -1, -2),
            new Vector3Int(-2, -1, 1),
            new Vector3Int(1, -1, -2),
            new Vector3Int(1, -1, 1),

            new Vector3Int(-2, -2, -2),
            new Vector3Int(-2, -2, 1),
            new Vector3Int(1, -2, -2),
            new Vector3Int(1, -2, 1),

            new Vector3Int(1, 2, -1),
            new Vector3Int(-2, 2, -1),
            new Vector3Int(-1, 2, 1),
            new Vector3Int(-1, 2, -2),
            new Vector3Int(1, 2, 0),
            new Vector3Int(-2, 2, 0),
            new Vector3Int(0, 2, 1),
            new Vector3Int(0, 2, -2),

            new Vector3Int(1, 1, -1),
            new Vector3Int(-2, 1, -1),
            new Vector3Int(-1, 1, 1),
            new Vector3Int(-1, 1, -2),
            new Vector3Int(1, 1, 0),
            new Vector3Int(-2, 1, 0),
            new Vector3Int(0, 1, 1),
            new Vector3Int(0, 1, -2),

            new Vector3Int(1, 0, -1),
            new Vector3Int(-2, 0, -1),
            new Vector3Int(-1, 0, 1),
            new Vector3Int(-1, 0, -2),
            new Vector3Int(1, 0, 0),
            new Vector3Int(-2, 0, 0),
            new Vector3Int(0, 0, 1),
            new Vector3Int(0, 0, -2),

            new Vector3Int(0, 1, 0),
            new Vector3Int(-1, 1, 0),
            new Vector3Int(0, 1, -1),
            new Vector3Int(-1, 1, -1),

            new Vector3Int(1, -1, -1),
            new Vector3Int(-2, -1, -1),
            new Vector3Int(-1, -1, 1),
            new Vector3Int(-1, -1, -2),
            new Vector3Int(1, -1, 0),
            new Vector3Int(-2, -1, 0),
            new Vector3Int(0, -1, 1),
            new Vector3Int(0, -1, -2),

            new Vector3Int(1, -2, -1),
            new Vector3Int(-2, -2, -1),
            new Vector3Int(-1, -2, 1),
            new Vector3Int(-1, -2, -2),
            new Vector3Int(1, -2, 0),
            new Vector3Int(-2, -2, 0),
            new Vector3Int(0, -2, 1),
            new Vector3Int(0, -2, -2),

            new Vector3Int(1, -3, -1),
            new Vector3Int(-2, -3, -1),
            new Vector3Int(-1, -3, 1),
            new Vector3Int(-1, -3, -2),
            new Vector3Int(1, -3, 0),
            new Vector3Int(-2, -3, 0),
            new Vector3Int(0, -3, 1),
            new Vector3Int(0, -3, -2),

            new Vector3Int(-1, -3, -1),
            new Vector3Int(-1, -3, 0),
            new Vector3Int(0, -3, -1),
            new Vector3Int(0, -3, 0),
        };
    }
}
