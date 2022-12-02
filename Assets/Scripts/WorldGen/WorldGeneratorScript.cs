﻿using Assets.Scripts.Utilities;
using Assets.Scripts.Village;
using Assets.Scripts.WorldGen.GenSteps;
using Assets.Scripts.WorldGen.RandomUpdaters;
using System.Collections.Generic;
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

        private readonly List<IGeneratorStep> Steps = new List<IGeneratorStep>
        {
            new PlaceTerrain(32, 52),
            new PlaceOres(0.001f),
            //new ChopBottom(),
            new PlaceTerrainDetails(20),
            new PlaceTrees(0.001f)
        };

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

            GlobalSettings.Instance.Map = new CubeMap(W, 200, D);
            GlobalSettings.Instance.Map.Updaters.Add(new GrassSpreadUpdater());
            newWatch.Stop();
            Debug.Log("Instantiating: " + newWatch.ElapsedMilliseconds);


            foreach (var step in Steps)
            {
                var perlinWatch = Stopwatch.StartNew();
                step.Commit(GlobalSettings.Instance.Map);
                perlinWatch.Stop();
                Debug.Log($"{step.GetType().Name}: {perlinWatch.ElapsedMilliseconds}");
            }

            GlobalSettings.Instance.Map.UpdateMeshes();
            
            fullWatch.Stop();
            Debug.Log("Full remake: " + fullWatch.ElapsedMilliseconds);
        }

        void SpawnVillagers()
        {
            Instantiate(GnomePrefab, new Vector3Int(9, 100, 9), Quaternion.identity);
        }

        void Update()
        {
            if (GlobalSettings.Instance == null || GlobalSettings.Instance.Map == null) return;
            GlobalSettings.Instance.Map.DrawMeshes(Camera.main, GlobalSettings.Instance.MapMaterial);
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