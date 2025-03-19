using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEditor;

public class LEGOExporter : MonoBehaviour
{
    [MenuItem("LEGO/Export LEGO Components")]
    public static void ExportLEGOComponents()
    {
        string legoPath = "Assets/LEGO Data/Geometry/New/LOD0";
        
        if (!AssetDatabase.IsValidFolder(legoPath))
        {
            Debug.LogError("Thư mục không tồn tại: " + legoPath);
            return;
        }
        
        // Lấy tất cả tài sản trong thư mục
        string[] guids = AssetDatabase.FindAssets("", new[] { legoPath });
        List<string> componentsInfo = new List<string>();
        
        componentsInfo.Add("Danh sách linh kiện LEGO:");
        componentsInfo.Add("--------------------------------");
        
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            
            if (asset != null)
            {
                string fileName = Path.GetFileName(assetPath);
                string fileType = asset.GetType().Name;
                componentsInfo.Add($"Tên: {fileName}, Loại: {fileType}, Đường dẫn: {assetPath}");
            }
        }
        
        // Xuất danh sách ra file
        string exportPath = Application.dataPath + "/../LEGOComponents.txt";
        File.WriteAllLines(exportPath, componentsInfo.ToArray());
        
        Debug.Log($"Đã xuất {guids.Length} linh kiện LEGO ra file: {exportPath}");
        
        // Mở file sau khi xuất
        Application.OpenURL("file://" + Path.GetFullPath(exportPath));
    }
}
