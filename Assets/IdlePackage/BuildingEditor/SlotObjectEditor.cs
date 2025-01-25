using UnityEditor;
using UnityEngine;

namespace Hiker.Idle
{
    public class SlotObjectEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Reset Slot"))
            {
                MonobehaviourCustom.DebugLog(name, "InitBuildSlots True", "Green", "Editor Log");
            }
        }
    }
}