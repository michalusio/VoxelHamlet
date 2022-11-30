using Assets.Scripts.PathFinding;
using Assets.Scripts.Utilities;
using Assets.Scripts.Village;
using Assets.Scripts.WorldGen;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Jobs
{
    public class PlaceBlockJob : Job
    {
        public Vector3Int Position { get; }

        public override Vector3Int RepresentantPosition => Position;
        public override VillagerRole ViableRoles => VillagerRole.Builder;

        private readonly List<Vector3Int> jobPositions;
        private readonly List<Vector3Int> validPositions;

        public readonly BlockType Block;

        public bool Done { get; private set; }

        public override bool AllowLaddering => true;

        public PlaceBlockJob(Vector3Int pos, BlockType block)
        {
            Position = pos;
            Block = block;
            jobPositions = new List<Vector3Int>(BuildJobPositions.Count);
            foreach (var p in BuildJobPositions)
            {
                var newPos = p + pos;
                if (GlobalSettings.Instance.Map.IsInBounds(newPos))
                {
                    jobPositions.Add(newPos);
                }
            }
            validPositions = new List<Vector3Int>(BuildJobPositions.Count);
        }

        public override void Commit(Villager e)
        {
            validPositions.Clear();
            foreach (var p in jobPositions)
            {
                if (BlockPathFinding.IsFreeForEntity(p))
                {
                    validPositions.Add(p);
                }
            }
            TryMoveOrReturn(e, validPositions, () =>
            {
                VillagerActions.PlaceBlock(e, Position, Block);
                Done = true;
                GlobalSettings.Instance.JobScheduler.JobDone(e);
            });
        }

        public override bool IsValid()
        {
            return GlobalSettings.Instance.Map[Position].BlockType == BlockType.Air &&
                GlobalSettings.Instance.JobScheduler.Storages.All(s => !s.Area.Inside(Position));
        }

        private static readonly List<Vector3Int> BuildJobPositions = new List<Vector3Int>
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
        };
    }
}
