using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public static Dictionary<DTNView, string> dsds;
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

        DTNViewInfo info = (DTNViewInfo)ViewInfoTable[viewName];

        return info.Address;
    }
}