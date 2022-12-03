using Assets.Scripts.WorldGen;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Editors
{
#if UNITY_EDITOR
    [CustomEditor(typeof(WorldGeneratorScript))]
    public class WorldGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();

            var generator = (WorldGeneratorScript) serializedObject.targetObject;

            if (GUILayout.Button("Remake world"))
            {
                generator.RemakeWorld();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
