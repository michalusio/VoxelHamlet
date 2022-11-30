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
            if (GUILayout.Button("Remake world"))
            {
                ((WorldGeneratorScript)serializedObject.targetObject).RemakeWorld();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
