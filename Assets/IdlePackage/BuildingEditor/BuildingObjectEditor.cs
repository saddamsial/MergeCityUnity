using UnityEditor;
using UnityEngine;

namespace Hiker.Idle
{
    [CustomEditor(typeof(BuildObject))]
    [CanEditMultipleObjects()]
    public class BuildingObjectEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Init Building Slots"))
            {
                var gameO = Selection.activeGameObject.GetComponent<BuildObject>();
                if (gameO)
                {
                    //if (gameO.InitBuildSlots(gameO.ObjBuildSlot))
                    //{
                    //    MonobehaviourCustom.DebugLog(name, "InitBuildSlots True", "Green", "Editor Log");
                    //}
                }
            }

            if (GUILayout.Button("Reset ALL Slot is Null"))
            {
                var gameO = Selection.activeGameObject.GetComponent<BuildObject>();
                if (gameO)
                {
                    //if (gameO.ResetAllSlotIsNull(gameO.ObjBuildSlot))
                    //{
                    //    MonobehaviourCustom.DebugLog(name, "InitBuildSlots True", "Green", "Editor Log");
                    //}
                }
            }
        }
    }
}