#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Global.CrowdSimulation.Testing
{
    [CustomEditor(typeof(ScenarioTesting), true), CanEditMultipleObjects]
    public class ScenarioEditor : UnityEditor.Editor
    {
        private ScenarioTesting _flock;

        private void OnEnable()
        {
            _flock = (ScenarioTesting)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Circle Distribution"))
                _flock.SpawnAgentsInCircle();

            if (GUILayout.Button("Reverse Goals"))
                _flock.ReverseDistribution();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Linear Distribution"))
                _flock.SpawnAgentsInLine();

            if (GUILayout.Button("Linear Goals"))
                _flock.LinearDistribution();

            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif