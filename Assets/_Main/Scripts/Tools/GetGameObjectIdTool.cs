using UnityEditor;
using UnityEngine;

/// <summary>
/// Tool component to get the Instance ID of the attached GameObject.
/// Attach this script to any GameObject to retrieve its unique Instance ID.
/// </summary>
public class GetGameObjectIdTool : MonoBehaviour
{
    public int GetId(GameObject gameObject)
    {
        return gameObject.GetInstanceID();
    }

    [Sirenix.OdinInspector.Button]
    public int LogId(GameObject gameObject)
    {
        int id = GetId(gameObject);
        Debug.Log($"[GetGameObjectIdTool] GameObject '{gameObject.name}' Instance ID: {id}", gameObject);
        return id;
    }
}

/// <summary>
/// Makes a field read-only in the Inspector (Editor only).
/// </summary>
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyInspectorAttribute))]
public class ReadOnlyInspectorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}
#endif

/// <summary>
/// Attribute to make a field read-only in the Inspector.
/// </summary>
public class ReadOnlyInspectorAttribute : PropertyAttribute { }
