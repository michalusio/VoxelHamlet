using Assets.Scripts.ConfigScripts;
using Assets.Scripts.Utilities;
using Assets.Scripts.Village;
using Assets.Scripts.WorldGen.GenSteps;
using Assets.Scripts.WorldGen.RandomUpdaters;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using Stopwatch = System.Diagnostics.Stopwatch;

namespace Assets.Scripts.WorldGen
{
    [ExecuteAlways]
    public class WorldGeneratorScript : MonoBehaviour
    {
        public GameObject GnomePrefab;
        // Referencing the singleton so it gets loaded by Unity by default
        [SerializeField] private GlobalSettings globalSettings;

        [Range(64, 16384)]
        public int W = 512;
        [Range(64, 16384)]
        public int D = 512;
        [Range(64, 256)]
        public int H = 128;

        private readonly int Seed = CrossSceneData.Seed;
        private readonly System.Random rng = new System.Random(CrossSceneData.Seed);
        private readonly int MountainModifier = CrossSceneData.MountainModifier;
        private readonly int ForestModifier = CrossSceneData.ForestModifier;

        private List<IGeneratorStep> Steps;

        void Start()
        {
            if (Application.isPlaying)
            {
                RemakeWorld();
                SpawnVillagers();
            }
        }

        public void RemakeWorld()
        {
            Steps = new List<IGeneratorStep>
            {
                new PlaceTerrain(Seed, MountainModifier, ForestModifier, 32, 64),
                new PlaceOres(rng, 0.001f),
                new PlaceTerrainDetails(rng, 20),
                new PlaceTrees(rng, 0.001f)
            };
            GlobalSettings.Instance.MapMaterial.SetColor("_AddColor", Color.white);
            GlobalSettings.Instance.MapMaterial.SetTexture("_TextureMapArray", GlobalSettings.Instance.TextureContainer.GetAlbedoArray());
            GlobalSettings.Instance.EditMapMaterial = new Material(GlobalSettings.Instance.MapMaterial);

            var fullWatch = Stopwatch.StartNew();
            var newWatch = Stopwatch.StartNew();

            transform.GetChild(0).localScale = new Vector3(W/256f, 1, D/256f);

            foreach(var gnome in FindObjectsOfType<Villager>())
            {
                Destroy(gnome.gameObject);
            }

            GlobalSettings.Instance.Map = new CubeMap(W, H, D);
            GlobalSettings.Instance.Map.Updaters.Add(new GrassSpreadUpdater());
            newWatch.Stop();
            DynamicLogger.Log("WorldGen", "Instantiating:", newWatch.ElapsedMilliseconds);

            foreach (var step in Steps)
            {
                var perlinWatch = Stopwatch.StartNew();
                step.Commit(GlobalSettings.Instance.Map);
                perlinWatch.Stop();
                DynamicLogger.Log("WorldGen", step.GetType().Name, perlinWatch.ElapsedMilliseconds);
            }

            GlobalSettings.Instance.Map.UpdateMeshes();
            
            fullWatch.Stop();
            DynamicLogger.Log("WorldGen", "Full remake:", fullWatch.ElapsedMilliseconds);
        }

        void SpawnVillagers()
        {
            Instantiate(GnomePrefab, new Vector3Int(9, 100, 9), Quaternion.identity);
        }

        void Update()
        {
            if (GlobalSettings.Instance == null || GlobalSettings.Instance.Map == null) return;
            GlobalSettings.Instance.Map.DrawMeshes(Camera.main, GlobalSettings.Instance.MapMaterial);
#if UNITY_EDITOR
            foreach (SceneView view in SceneView.sceneViews)
            {
                if (view.hasFocus) {
                    GlobalSettings.Instance.Map.DrawMeshes(view.camera, GlobalSettings.Instance.MapMaterial);
                }
            }
#endif
            if (Time.timeScale > 0.5f)
            {
                GlobalSettings.Instance.Map.RunUpdaters();
            }
        }

        void LateUpdate()
        {
            if (GlobalSettings.Instance == null || GlobalSettings.Instance.Map == null) return;
            GlobalSettings.Instance.Map.UpdateMeshes();
        }
    }
}