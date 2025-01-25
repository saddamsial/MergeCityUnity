using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TestManager))]
public class TestManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (Application.isPlaying)
        {
            if (GUILayout.Button("Show Board"))
            {
                var testManager = (TestManager)target;
                testManager.ShowBoard();
            }
        }
    }
}
