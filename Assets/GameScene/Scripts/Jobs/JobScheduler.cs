using Assets.Scripts.ConfigScripts;
using Assets.Scripts.PathFinding;
using Assets.Scripts.Utilities;
using Assets.Scripts.Utilities.Math;
using Assets.Scripts.Village;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Jobs
{
    public class JobScheduler : MonoBehaviour
    {
        public Mesh JobMesh;
        public Material JobRenderMaterial;
        public Material StorageRenderMaterial;

        private bool UpdateJobRender = true;
        private InstanceBatcher jobRenderBatch;

        private bool UpdateStorageRender = true;
        private InstanceBatcher storageRenderBatch;

        public Dictionary<Villager, Job> VillagerJobs = new Dictionary<Villager, Job>();
        private Villager[] Villagers;
        public Dictionary<VillagerRole, List<Job>> JobQueue = new Dictionary<VillagerRole, List<Job>>();
        public List<Storage> Storages = new List<Storage>();

        void Start()
        {
            GlobalSettings.Instance.JobScheduler = this;
            Villagers = FindObjectsOfType<Villager>();
            jobRenderBatch = new InstanceBatcher
            {
                Mesh = JobMesh,
                Materials = new[] { JobRenderMaterial }
            };
            storageRenderBatch = new InstanceBatcher
            {
                Mesh = JobMesh,
                Materials = new[] { StorageRenderMaterial }
            };
            viableJobs = new List<(Job, int, float)>(100);
            viableRoles = new List<VillagerRole>(10);
        }

        private void UpdateVillagers()
        {
            Villagers = FindObjectsOfType<Villager>();
        }

        private readonly Queue<Job> JobAddQueue = new Queue<Job>();
        public void AddAreaJob(Vector3Int areaStart, Vector3Int areaEnd, System.Func<Vector3Int, Job> jobCreator)
        {
            var temp = Vector3Int.Max(areaStart, areaEnd);
            areaStart = Vector3Int.Min(areaStart, areaEnd);
            areaEnd = temp;
            Vector3Int pos = default;
            for (int x = areaStart.x; x <= areaEnd.x; x++)
            {
                pos.x = x;
                for (int z = areaStart.z; z <= areaEnd.z; z++)
                {
                    pos.z = z;
                    for (int y = areaStart.y; y <= areaEnd.y; y++)
                    {
                        pos.y = y;
                        JobAddQueue.Enqueue(jobCreator(pos));
                    }
                }
            }
        }

        public void AddStorage(Storage storage)
        {
            if (Storages.All(s => !s.Area.Intersects(storage.Area)))
            {
                Storages.Add(storage);
                UpdateStorageRender = true;
            }
        }

        public bool AddJob(Job job)
        {
            if (!job.IsValid()) return false;
            if (!JobQueue.ContainsKey(job.ViableRoles)) JobQueue[job.ViableRoles] = new List<Job>();
            var sameTypeJobs = JobQueue[job.ViableRoles];
            if (sameTypeJobs.Any(j => j.RepresentantPosition == job.RepresentantPosition))
            {
                return false;
            }
            else
            {
                JobQueue[job.ViableRoles].Add(job);
                UpdateJobRender = true;
                return true;
            }
        }

        internal void JobDone(Villager e)
        {
            VillagerJobs[e] = null;
            e.AllowLaddering = false;
            UpdateJobRender = true;
        }
        internal void ReturnJobOf(Villager e)
        {
            var job = VillagerJobs[e];
            e.AllowLaddering = false;
            e.SetGoal(new List<Vector3Int> { e.CurrentPosition });
            job.Penalize();
            JobQueue[job.ViableRoles].Add(VillagerJobs[e]);
            VillagerJobs[e] = null;
        }

        void Update()
        {
            GlobalSettings.Instance.ItemManager.Update();

            int tries = 20;
            while (tries > 0 && JobAddQueue.Count > 0)
            {
                AddJob(JobAddQueue.Dequeue());
                tries--;
            }

            foreach (var villager in Villagers)
            {
                if (!VillagerJobs.ContainsKey(villager)) VillagerJobs[villager] = default;
                var job = VillagerJobs[villager];
                if (job == null)
                {
                    if (villager.FreeToWork)
                    {
                        job = FindJobFor(villager);
                        if (job != null) job.Commit(villager);
                    }
                }
                else
                {
                    job.Commit(villager);
                }
            }
        }

        void LateUpdate()
        {
            RenderJobs();
            GlobalSettings.Instance.ItemManager.RenderItems();
            GlobalSettings.Instance.EntityManager.RenderEntities();
        }

        private List<(Job, int, float)> viableJobs;
        private List<VillagerRole> viableRoles;
        private Job FindJobFor(Villager villager)
        {
            viableJobs.Clear();
            viableRoles.Clear();
            foreach (var kv in JobQueue)
            {
                if ((kv.Key & villager.Role) == 0) continue;
                viableRoles.Add(kv.Key);
            }
            viableRoles.Sort();

            foreach(var role in viableRoles)
            {
                var kv = JobQueue[role];
                int jobLimit = Mathf.CeilToInt(kv.Count / 2f);
                for (int i = 0; i < kv.Count; i++)
                {
                    var job = kv[i];
                    viableJobs.Add((job, i, BlockPathFinding.Heuristic(job.RepresentantPosition, villager.CurrentPosition) + job.Penalty));
                    if (viableJobs.Count > jobLimit) break;
                }
                //if (viableJobs.Count > 0) break;
            }
            if (viableJobs.Count == 0) return null;

            var chosenJob = viableJobs[0];
            for (int i = 1; i < viableJobs.Count; i++)
            {
                var nextJob = viableJobs[i];
                if (nextJob.Item3 < chosenJob.Item3)
                {
                    chosenJob = nextJob;
                }
            }
            JobQueue[chosenJob.Item1.ViableRoles].RemoveAt(chosenJob.Item2);
            VillagerJobs[villager] = chosenJob.Item1;
            villager.AllowLaddering = chosenJob.Item1.AllowLaddering;
            return chosenJob.Item1;
        }

       

        private readonly List<Matrix4x4> JobPositions = new List<Matrix4x4>(1024);
        private readonly List<Vector4> JobColors = new List<Vector4>(1024);
        public void RenderJobs()
        {
            var halfVector = new Vector3(0.5f, 0.5f, 0.5f);

            var jobCheck = JobQueue.Count > 0;
            if (!jobCheck)
            {
                foreach(var v in VillagerJobs)
                {
                    if (v.Value != null)
                    {
                        jobCheck = true;
                        break;
                    }
                }
            }

            if (jobCheck)
            {
                if (UpdateJobRender)
                {
                    JobPositions.Clear();
                    JobColors.Clear();

                    foreach (var kv in JobQueue)
                    {
                        foreach (var job in kv.Value)
                        {
                            JobPositions.Add(Matrix4x4.TRS(job.RepresentantPosition + halfVector, Quaternion.identity, halfVector));
                            JobColors.Add(Job.JobColorsDictionary[job.GetType()]);
                        }
                    }
                    foreach (var kv in VillagerJobs)
                    {
                        if (kv.Value != null)
                        {
                            JobPositions.Add(Matrix4x4.TRS(kv.Value.RepresentantPosition + halfVector, Quaternion.identity, halfVector));
                            JobColors.Add(Job.JobColorsDictionary[kv.Value.GetType()]);
                        }
                    }
                    jobRenderBatch.Rebatch(JobPositions, "_Color", JobColors);
                    UpdateJobRender = false;
                }
                var layer = LayerMask.NameToLayer("Jobs");
                jobRenderBatch.Render(layer);
            }

            if (Storages.Count > 0)
            {
                if (UpdateStorageRender)
                {
                    var halfx0z = halfVector._x0z();
                    halfx0z.y += 0.05f;
                    var onex0z = Vector3.one._x0z();
                    var storagePositions = Storages.Select(s => Matrix4x4.TRS((s.Area.Min + (Vector3)s.Area.Max) / 2 + halfx0z, Quaternion.identity, s.Area.Max - s.Area.Min + onex0z)).ToList();
                    var storageSizes = Storages.Select(s => ((Vector3)s.Area.Size).ToVector4()).ToList();
                    storageRenderBatch.Rebatch(storagePositions, "_Size", storageSizes);
                    UpdateStorageRender = false;
                }
                var layer = LayerMask.NameToLayer("Storages");
                storageRenderBatch.Render(layer);
            }
        }

    }
}