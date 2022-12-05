using Assets.Scripts.Jobs;
using UnityEditor;

namespace Assets.Scripts.Editors
{
#if UNITY_EDITOR
    [CustomEditor(typeof(JobScheduler))]
    public class JobSchedulerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Jobs in queue: ");
            var queue = (serializedObject.targetObject as JobScheduler).JobQueue;
            if (queue != null)
            {
                foreach(var kv in queue)
                {
                    EditorGUILayout.LabelField(kv.Key.ToString() + ": ", kv.Value.Count.ToString());
                }
            }
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Jobs in villagers:");
            var jobs = (serializedObject.targetObject as JobScheduler).VillagerJobs;
            if (jobs != null)
            {
                foreach (var j in jobs)
                {
                    EditorGUILayout.LabelField(j.Value?.ToString() ?? "None");
                }
            }

            serializedObject.ApplyModifiedProperties();

        }

        public override bool RequiresConstantRepaint()
        {
            return true;
        }
    }
#endif
}
