
#if UNITY_EDITOR
using System.Collections;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DTNDailyRewardManager))]
public class DTNCustomDTNDailyRewardManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DTNDailyRewardManager target_ = (DTNDailyRewardManager)target;
        if (GUILayout.Button("Delete Data"))
        {
            target_.ResetData();
        }
    }
}
#endif

