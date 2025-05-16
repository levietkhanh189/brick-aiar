using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class LDrawImporter : MonoBehaviour
{
    [SerializeField] private string ldrawFilePath = "Assets/_Main/Scripts/Lego/v7.ldr";
    [SerializeField] private Vector3 scaleFactor = new Vector3(0.05f, 0.05f, 0.05f); // Tỷ lệ chuyển đổi từ LDU sang Unity
    public List<Material> materials = new List<Material>();
    private GameObject currentModel;

    [SerializeField] private bool isDeleteOld = true;
    
    [SerializeField] private bool invertYAxis = true;

    [Sirenix.OdinInspector.Button]
    void Import()
    {
        ReloadModel();
    }


    private void ReloadModel()
    {
        if (isDeleteOld && currentModel != null)
        {
            DestroyImmediate(currentModel);
        }
        currentModel = new GameObject("Model");
        currentModel.transform.SetParent(transform, false);
        ImportLDrawModel(ldrawFilePath);
    }

    public void ImportLDrawModel(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError($"Tệp LDraw không tồn tại: {filePath}");
            return;
        }

        if (currentModel == null)
        {
            currentModel = new GameObject("Model");
            currentModel.transform.SetParent(transform, false);
        }
        else
        {
            foreach (Transform child in currentModel.transform)
            {
                DestroyImmediate(child.gameObject);
            }
        }

        string[] lines = File.ReadAllLines(filePath);
        foreach (string line in lines)
        {
            ProcessLDrawLine(line);
        }
    }

    private void ProcessLDrawLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("1"))
            return;

        string[] parts = line.Split(' ').Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
        if (parts.Length < 15)
        {
            Debug.LogWarning($"Dòng LDraw không hợp lệ: {line}");
            return;
        }

        int colorCode = int.Parse(parts[1]);
        
        // Vị trí trong LDraw
        Vector3 position = new Vector3(
            float.Parse(parts[2]),
            float.Parse(parts[3]),
            float.Parse(parts[4])
        );
        
        // Ma trận biến đổi từ LDraw - LDraw sử dụng hệ tọa độ tay phải với -Y hướng lên
        // (hàng 1 - trục X)
        float a = float.Parse(parts[5]);
        float b = float.Parse(parts[6]);
        float c = float.Parse(parts[7]);
        // (hàng 2 - trục Y)
        float d = float.Parse(parts[8]);
        float e = float.Parse(parts[9]);
        float f = float.Parse(parts[10]);
        // (hàng 3 - trục Z)
        float g = float.Parse(parts[11]);
        float h = float.Parse(parts[12]);
        float i = float.Parse(parts[13]);
        
        // Tạo ma trận biến đổi
        Matrix4x4 matrix = new Matrix4x4(
            new Vector4(a, b, c, 0),
            new Vector4(d, e, f, 0),
            new Vector4(g, h, i, 0),
            new Vector4(0, 0, 0, 1)
        );
        
        string datFile = parts[14].Replace(".dat", "");

        GameObject prefab = Resources.Load<GameObject>($"Prefabs/{datFile}");
        if (prefab == null)
        {
            Debug.LogWarning($"Không tìm thấy Prefab cho {datFile}");
            return;
        }

        GameObject instance = Instantiate(prefab, currentModel.transform);
        instance.name = datFile;

        // Chuyển đổi vị trí và ma trận từ LDraw sang Unity
        Vector3 unityPosition = ConvertLDrawToUnityPosition(position);
        
        // Áp dụng tỷ lệ
        unityPosition.x *= scaleFactor.x;
        unityPosition.y *= scaleFactor.y;
        unityPosition.z *= scaleFactor.z;
        
        // Đặt vị trí cho instance
        instance.transform.localPosition = unityPosition;
        
        // Chuyển đổi ma trận biến đổi và thiết lập góc xoay
        Quaternion rotation = ConvertLDrawToUnityRotation(matrix);
        instance.transform.localRotation = rotation;
        Material material = materials.Find(m => m.name == "LegoColor_" + colorCode);
        if (material != null)
        {
            instance.GetComponent<LegoController>().Init(material);
        }
        else
        {
            Debug.LogWarning($"Mã màu LDraw {"LegoColor_" + colorCode} không được định nghĩa.");
        }
    }

    private Vector3 ConvertLDrawToUnityPosition(Vector3 ldrawPos)
    {
        // LDraw: Hệ tọa độ tay phải, +X sang phải, +Y hướng lên, +Z ra ngoài màn hình
        // Unity: Hệ tọa độ tay trái, +X sang phải, +Y hướng lên, +Z hướng vào màn hình
        
        // Cần đảo ngược trục Z để chuyển từ hệ tọa độ tay phải sang tay trái
        float y = invertYAxis ? -ldrawPos.y : ldrawPos.y;
        return new Vector3(ldrawPos.x, y, -ldrawPos.z);
    }

    private Quaternion ConvertLDrawToUnityRotation(Matrix4x4 ldrawMatrix)
    {
        // Tạo ma trận chuyển đổi để đảo chiều trục Z (và Y nếu cần)
        Matrix4x4 conversionMatrix = Matrix4x4.identity;
        conversionMatrix.m22 = -1; // Đảo chiều trục Z
        
        if (invertYAxis)
            conversionMatrix.m11 = -1; // Đảo chiều trục Y nếu cần
        
        // Kết hợp ma trận chuyển đổi với ma trận LDraw
        Matrix4x4 unityMatrix = conversionMatrix * ldrawMatrix;
        
        // Chuyển ma trận thành quaternion
        // Đảm bảo ma trận là ma trận xoay hợp lệ
        unityMatrix.m03 = 0;
        unityMatrix.m13 = 0;
        unityMatrix.m23 = 0;
        unityMatrix.m33 = 1;
        
        // Chuyển đổi thành quaternion
        return ExtractRotationFromMatrix(unityMatrix);
    }

    private Quaternion ExtractRotationFromMatrix(Matrix4x4 matrix)
    {
        // Thuật toán chuyển đổi ma trận thành quaternion cải tiến
        // Dựa trên các nghiên cứu về chuyển đổi ma trận sang quaternion
        
        float trace = matrix.m00 + matrix.m11 + matrix.m22;
        Quaternion q = new Quaternion();
        
        if (trace > 0)
        {
            float s = Mathf.Sqrt(trace + 1.0f) * 2;
            q.w = 0.25f * s;
            q.x = (matrix.m21 - matrix.m12) / s;
            q.y = (matrix.m02 - matrix.m20) / s;
            q.z = (matrix.m10 - matrix.m01) / s;
        }
        else if (matrix.m00 > matrix.m11 && matrix.m00 > matrix.m22)
        {
            float s = Mathf.Sqrt(1.0f + matrix.m00 - matrix.m11 - matrix.m22) * 2;
            q.w = (matrix.m21 - matrix.m12) / s;
            q.x = 0.25f * s;
            q.y = (matrix.m01 + matrix.m10) / s;
            q.z = (matrix.m02 + matrix.m20) / s;
        }
        else if (matrix.m11 > matrix.m22)
        {
            float s = Mathf.Sqrt(1.0f + matrix.m11 - matrix.m00 - matrix.m22) * 2;
            q.w = (matrix.m02 - matrix.m20) / s;
            q.x = (matrix.m01 + matrix.m10) / s;
            q.y = 0.25f * s;
            q.z = (matrix.m12 + matrix.m21) / s;
        }
        else
        {
            float s = Mathf.Sqrt(1.0f + matrix.m22 - matrix.m00 - matrix.m11) * 2;
            q.w = (matrix.m10 - matrix.m01) / s;
            q.x = (matrix.m02 + matrix.m20) / s;
            q.y = (matrix.m12 + matrix.m21) / s;
            q.z = 0.25f * s;
        }
        
        return q.normalized; // Đảm bảo quaternion được chuẩn hóa
    }
}