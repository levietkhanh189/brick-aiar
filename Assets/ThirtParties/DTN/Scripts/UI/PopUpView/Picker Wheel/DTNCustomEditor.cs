#if UNITY_EDITOR
using System.Collections;
using UnityEngine;
using UnityEditor;


namespace DTNUIManagerV2
{
    [CustomEditor(typeof(DTNPickerWheelPopUp))]
    public class DTNCustomEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            DTNPickerWheelPopUp PickerWheelPopUp = (DTNPickerWheelPopUp)target;
            if (GUILayout.Button("Generate Wheel"))
            {
                PickerWheelPopUp.GenerateWheel();
            }
        }
    }
}

#endif

