using Assets.Scripts.EditMode;
using Assets.Scripts.Items;
using Assets.Scripts.Jobs;
using Assets.Scripts.PathFinding;
using Assets.Scripts.WorldGen;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.ConfigScripts
{
    [CreateAssetMenu(fileName = "GlobalSettingsDB", menuName = "GlobalSettings", order = 0)]
    public class GlobalSettings : ScriptableObject
    {
        private static GlobalSettings _instance;
        public static GlobalSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.FindObjectsOfTypeAll<GlobalSettings>().FirstOrDefault();
                    _instance.SetUp();
                }
                return _instance;
            }
        }

        public static GlobalVariables Variables => Instance.Settings;

        [NonSerialized] public CubeMap Map;
        [NonSerialized] public EditModeController EditMode;

        public GlobalVariables Settings;
        public NavMesh NavMesh;
        public ItemManager ItemManager;
        public EntityManager EntityManager;
        public Material MapMaterial;
        public Material EditMapMaterial;
        public Texture2DArrayContainer TextureContainer;
        [NonSerialized] public JobScheduler JobScheduler;

        [NonSerialized] public string BuildingsPath;

        public GameObject LadderPrefab;

        public Mesh HandleMesh;
        public Mesh AreaHandleMesh;
        public Material HandleMaterial;

        private void SetUp()
        {
            NavMesh = FindObjectOfType<NavMesh>();
            BuildingsPath = Path.Combine(Application.dataPath, "Buildings/");
            if (!Directory.Exists(BuildingsPath))
            {
                Directory.CreateDirectory(BuildingsPath);
            }
        }
    }
}
