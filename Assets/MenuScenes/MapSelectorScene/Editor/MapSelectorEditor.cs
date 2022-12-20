using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(MapSelectorScript))]
public class MapSelectorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();

        var generator = (MapSelectorScript)serializedObject.targetObject;

        if (GUILayout.Button("Generate new terrain"))
        {
            generator.GenerateSeed();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
