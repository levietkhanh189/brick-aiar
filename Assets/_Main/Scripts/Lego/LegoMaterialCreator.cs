using System.Collections.Generic;
using UnityEngine;

public class LegoMaterialCreator : MonoBehaviour
{
    // Dictionary để lưu trữ màu sắc theo ID
    private readonly Dictionary<int, Color> legoColors = new Dictionary<int, Color>
    {
        { 0, new Color(0.2f, 0.2f, 0.2f) }, // Đen
        { 1, new Color(0, 0.5f, 0.9f) },    // Xanh dương
        { 7, new Color(0.7f, 0.7f, 0.7f) }, // Xám
        { 14, new Color(1, 0.9f, 0) },      // Vàng
        { 15, new Color(1, 1, 1) },         // Trắng
        { 4, new Color(0.7f, 0, 0) },       // Đỏ
        { 2, new Color(0, 0.7f, 0) }        // Xanh lá
    };

    // Hàm để lưu material vào Assets (chỉ gọi trong Editor nếu cần)
    [Sirenix.OdinInspector.Button]
    public void SaveMaterials()
    {
#if UNITY_EDITOR
        string folderPath = "Assets/_Main/Resources/Materials";
        // Tạo thư mục nếu chưa tồn tại
        if (!System.IO.Directory.Exists(folderPath))
        {
            System.IO.Directory.CreateDirectory(folderPath);
        }

        foreach (var colorPair in legoColors)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = colorPair.Value;
            mat.name = "LegoColor_" + colorPair.Key;
            UnityEditor.AssetDatabase.CreateAsset(mat, folderPath + "/LegoColor_" + colorPair.Key + ".mat");
        }
        UnityEditor.AssetDatabase.SaveAssets();
        Debug.Log("Materials đã được lưu vào " + folderPath);
#endif
    }
}