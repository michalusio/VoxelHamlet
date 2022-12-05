using Assets.Scripts.ConfigScripts;
using UnityEngine;

namespace Assets.Scripts.TimeScripts
{
    [RequireComponent(typeof(Light))]
    public class DayNightCycle : MonoBehaviour
    {
        public InGameDateTime TimeAndDate;

        public float TimeSpeed = 60; //1 second irl = TimeSpeed seconds in-game

        private Light sunLight;

        void Start()
        {
            sunLight = GetComponent<Light>();
        }

        void Update()
        {
            var inst = GlobalSettings.Instance;
            if (inst.Map == null) return;
            var HalfMapSize = new Vector3(inst.Map.W, inst.Map.H, inst.Map.D) / 2;

            TimeAndDate = TimeAndDate.AddDelta(TimeSpeed * Time.deltaTime);

            transform.position = TimeAndDate.GetSunPosition() * 100 + HalfMapSize;
            transform.forward = -(transform.position - HalfMapSize).normalized;

            sunLight.intensity = Mathf.Cos(TimeAndDate.GetSunInclination() * Mathf.Deg2Rad);

            sunLight.enabled = TimeAndDate.Hours > 5 && TimeAndDate.Hours < 19;
        }
    }
}