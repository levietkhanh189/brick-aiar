using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector;
using System.Text.RegularExpressions;
#endif

[System.Serializable]
public class DTNViewInfo
{
    [SerializeField]
    public string ViewName;
    [SerializeField]
    public string Address = "Views/";
}

[System.Serializable]
[CreateAssetMenuAttribute(fileName = "ViewInfoSystem", menuName = "Data/Scriptable/View Info System")]
public class DTNViewInfoSystem : ScriptableObject
{
    public List<DTNViewInfo> ViewInfos;
    Hashtable ViewInfoTable = new Hashtable();
    void CreateHashTable()
    {
        for (int i = 0; i < ViewInfos.Count; i++)
        {
            ViewInfoTable.Add(ViewInfos[i].ViewName, ViewInfos[i]);
        }
    }

    public string GetStringAddress(string viewName)
    {
        if (ViewInfoTable.Count <= 0)
        {
            CreateHashTable();
        }
        Debug.Log("DTNViewInfo = " + viewName);
        DTNViewInfo info = (DTNViewInfo)ViewInfoTable[viewName];

        return info.Address;
    }

#if UNITY_EDITOR
[Button("Auto Fetch Prefabs in Resources/Views")]
public void AutoFetchPrefabs()
{
    ViewInfos = new List<DTNViewInfo>();
    HashSet<string> scriptNames = GetAllScriptNames();

    string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Main/Resources/Views" });
    foreach (string guid in guids)
    {
        string assetPath = AssetDatabase.GUIDToAssetPath(guid);
        HandlePrefab(assetPath, scriptNames);
    }

    EditorUtility.SetDirty(this);
    AssetDatabase.SaveAssets();
    Debug.Log("Auto fetch completed. Found: " + ViewInfos.Count + " prefabs.");
}

/// <summary>
/// Lấy danh sách tất cả tên script trong dự án (không extension)
/// </summary>
private HashSet<string> GetAllScriptNames()
{
    string[] scriptGuids = AssetDatabase.FindAssets("t:Script");
    HashSet<string> scriptNames = new HashSet<string>();
    foreach (string scriptGuid in scriptGuids)
    {
        string scriptPath = AssetDatabase.GUIDToAssetPath(scriptGuid);
        string scriptName = System.IO.Path.GetFileNameWithoutExtension(scriptPath);
        scriptNames.Add(scriptName);
    }
    return scriptNames;
}

/// <summary>
/// Xử lý từng prefab: chuẩn hóa tên, đổi tên file nếu cần, kiểm tra script, thêm vào ViewInfos
/// </summary>
private void HandlePrefab(string assetPath, HashSet<string> scriptNames)
{
    string prefabName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
    string cleanedName = CleanName(prefabName);

    RenamePrefabIfNeeded(assetPath, prefabName, cleanedName, out string updatedAssetPath);

    WarnIfScriptNotFound(cleanedName, scriptNames);

    ViewInfos.Add(new DTNViewInfo
    {
        ViewName = cleanedName,
        Address = "Views/" + cleanedName
    });
}

/// <summary>
/// Xoá dấu cách trong tên
/// </summary>
private string CleanName(string name)
{
    return Regex.Replace(name, @"\s+", "");
}

/// <summary>
/// Đổi tên file prefab nếu tên chuẩn hóa khác tên gốc
/// </summary>
private void RenamePrefabIfNeeded(string assetPath, string prefabName, string cleanedName, out string updatedAssetPath)
{
    updatedAssetPath = assetPath;
    if (cleanedName != prefabName)
    {
        string newAssetPath = System.IO.Path.GetDirectoryName(assetPath) + "/" + cleanedName + ".prefab";
        string error = AssetDatabase.RenameAsset(assetPath, cleanedName);
        if (!string.IsNullOrEmpty(error))
        {
            Debug.LogWarning($"[DTNViewInfoSystem] Rename failed for prefab {prefabName}: {error}");
        }
        else
        {
            Debug.Log($"[DTNViewInfoSystem] Renamed prefab: {prefabName} -> {cleanedName}");
            updatedAssetPath = newAssetPath;
        }
    }
}

/// <summary>
/// Cảnh báo nếu không tìm thấy script cùng tên
/// </summary>
private void WarnIfScriptNotFound(string cleanedName, HashSet<string> scriptNames)
{
    if (!scriptNames.Contains(cleanedName))
    {
        Debug.LogWarning($"[DTNViewInfoSystem] Script not found for view: {cleanedName}");
    }
}
#endif

}