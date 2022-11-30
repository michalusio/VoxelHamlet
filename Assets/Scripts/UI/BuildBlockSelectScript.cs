using Assets.Scripts.Entities;
using Assets.Scripts.Jobs;
using Assets.Scripts.Utilities;
using Assets.Scripts.WorldGen;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    [RequireComponent(typeof(ToggleGroup))]
    public class BuildBlockSelectScript : MonoBehaviour
    {
        private ToggleGroup group;

        void Start()
        {
            group = GetComponent<ToggleGroup>();
        }

        internal (Block, Entity)? GetBlockOrEntity()
        {
            var toggle = group.ActiveToggles().FirstOrDefault();
            if (toggle == null) return null;
            var blockName = toggle.name.Split(' ')[1];
            Block b = default;
            b.BlockType = (BlockType) Enum.Parse(typeof(BlockType), blockName);
            return (b, null);
        }

        public void SaveStructure()
        {
            var building = BuildingContainer.FromMap(GlobalSettings.Instance.EditMode.EditMap, GlobalSettings.Instance.EditMode.EditSize);

            (var averagePosition, var averageDistribution) = building.GetBlockStatistics();
            Debug.Log("Average Pos: " + averagePosition);
            Debug.Log("Distribution: " + averageDistribution);

            GlobalSettings.Instance.EditMode.EditScreenshotCamera.transform.localPosition = averagePosition - averageDistribution * 2 * GlobalSettings.Instance.EditMode.ScreenshotForward;

            StartCoroutine(SavingCoroutine(building));
        }

        private IEnumerator SavingCoroutine(BuildingContainer building)
        {
            yield return new WaitForSecondsRealtime(0.5f);
            RenderTexture.active = GlobalSettings.Instance.EditMode.EditScreenshotCamera.targetTexture;
            var icon = new Texture2D(256, 256, TextureFormat.RGB24, false, true);
            icon.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);
            icon.Apply();

            building.Icon = icon;

            var assets = Directory.GetFiles(GlobalSettings.Instance.BuildingsPath, "*.building")
                .Select(Path.GetFileNameWithoutExtension)
                .ToList();
            var fileName = "Building";
            var index = 0;
            while (assets.Contains(fileName + index))
            {
                index++;
            }
            File.WriteAllBytes(Path.Combine(GlobalSettings.Instance.BuildingsPath, (fileName + index) + ".building"), building.GetByteData());
            GlobalSettings.Instance.EditMode.BuildSelectScript.PopulateWithBuildings(GlobalSettings.Instance.BuildingsPath);
        }

        public void BuildStructure()
        {
            var map = GlobalSettings.Instance.EditMode.EditMap;
            var relPos = Vector3Int.RoundToInt(GlobalSettings.Instance.EditMode.transform.position);
            GlobalSettings.Instance.EditMode.StartCoroutine(AddBuildingJobs(map, relPos));
            GlobalSettings.Instance.EditMode.ToggleEditMode();
        }

        private IEnumerator AddBuildingJobs(CubeMap map, Vector3Int relPos)
        {
            var jobsAdded = new List<PlaceBlockJob>();
            var jobsNotYetAdded = new Dictionary<int, List<PlaceBlockJob>>();
            foreach (var kv in map.GetChunks.SelectMany(c => c.Value.GetGreedyCubes().Select(g => (c.Key, g))))
            {
                var relChunkPos = relPos + kv.Key * CubeMap.RegionSize;

                Vector3Int pos = default;
                for (int y = relChunkPos.y + kv.g.sy; y <= relChunkPos.y + kv.g.ey; y++)
                {
                    pos.y = y;
                    if (!jobsNotYetAdded.ContainsKey(y)) jobsNotYetAdded[y] = new List<PlaceBlockJob>();
                    for (int x = relChunkPos.x + kv.g.sx; x <= relChunkPos.x + kv.g.ex; x++)
                    {
                        pos.x = x;
                        for (int z = relChunkPos.z + kv.g.sz; z <= relChunkPos.z + kv.g.ez; z++)
                        {
                            pos.z = z;
                        
                            jobsNotYetAdded[y].Add(new PlaceBlockJob(pos, kv.g.id));
                        }
                    }
                }
            }

            while (jobsNotYetAdded.Count > 0)
            {
                
                while ((jobsAdded.Count(j => !j.Done) / (float)jobsAdded.Count) > 0.05f)
                {
                    yield return new WaitForSeconds(1f);
                }
                var minY = jobsNotYetAdded.Min(kv => kv.Key);
                jobsAdded = jobsNotYetAdded[minY];
                jobsNotYetAdded.Remove(minY);
                foreach (var job in jobsAdded)
                {
                    GlobalSettings.Instance.JobScheduler.AddJob(job);
                }
            }
        }
    }
}