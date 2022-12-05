using Assets.Scripts.TimeScripts;
using UnityEditor;

namespace Assets.Scripts.Editors
{
#if UNITY_EDITOR
    [CustomEditor(typeof(DayNightCycle))]
    public class DayNightCycleEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var cycle = (DayNightCycle)serializedObject.targetObject;

            DrawDefaultInspector();

            EditorGUILayout.LabelField("Time: ", cycle.TimeAndDate.FormatTime());
            EditorGUILayout.LabelField("Date: ", cycle.TimeAndDate.FormatDate());
            EditorGUILayout.LabelField("Inclination: ", cycle.TimeAndDate.GetSunInclination().ToString());

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
