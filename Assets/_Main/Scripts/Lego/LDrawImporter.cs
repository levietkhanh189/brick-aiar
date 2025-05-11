using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class LDrawImporter : MonoBehaviour
{
    [SerializeField] private string ldrawFilePath = "Assets/_Main/Scripts/Lego/test.ldr";
    [SerializeField] private Vector3 scaleFactor = new Vector3(0.05f, 0.05f, 0.05f); // Tỷ lệ chuyển đổi từ LDU sang Unity

    // Dictionary lưu offset trọng tâm cho từng .dat (dựa trên LDraw)
    private Dictionary<string, Vector3> prefabPivotOffsets = new Dictionary<string, Vector3>
    {
        { "3005", new Vector3(0, -1.2f, 0) }, // Ví dụ: trọng tâm của 3005.dat lệch -1.2 trên trục Y
        { "3941", new Vector3(0, -0.8f, 0) }  // Ví dụ: trọng tâm của 3941.dat lệch -0.8 trên trục Y
    };

    // Bảng ánh xạ màu LDraw
    private Dictionary<int, Color> ldrawColors = new Dictionary<int, Color>
    {
        { 0, Color.black },
        { 1, Color.blue },
        { 7, Color.gray },
        { 14, Color.yellow },
        { 15, Color.white }
    };

    private GameObject currentModel;

    [SerializeField] private bool isDeleteOld = true;

    [Sirenix.OdinInspector.Button]
    void Import()
    {
        ImportLDrawModel(ldrawFilePath);
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
        if (parts.Length != 15)
        {
            Debug.LogWarning($"Dòng LDraw không hợp lệ: {line}");
            return;
        }

        int colorCode = int.Parse(parts[1]);
        Vector3 position = new Vector3(
            float.Parse(parts[2]) * scaleFactor.x,
            float.Parse(parts[3]) * scaleFactor.y,
            float.Parse(parts[4]) * scaleFactor.z
        );
        Matrix4x4 matrix = new Matrix4x4(
            new Vector4(float.Parse(parts[5]), float.Parse(parts[8]), float.Parse(parts[11]), 0),
            new Vector4(float.Parse(parts[6]), float.Parse(parts[9]), float.Parse(parts[12]), 0),
            new Vector4(float.Parse(parts[7]), float.Parse(parts[10]), float.Parse(parts[13]), 0),
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

        // Áp dụng offset trọng tâm từ Dictionary
        Vector3 pivotOffset = prefabPivotOffsets.ContainsKey(datFile) ? prefabPivotOffsets[datFile] : Vector3.zero;
        instance.transform.localPosition = ConvertToUnityCoordinates(position) + pivotOffset;

        Quaternion rotation = MatrixToQuaternion(matrix);
        instance.transform.localRotation = rotation;

        Vector3 scale = new Vector3(
            matrix.GetColumn(0).magnitude,
            matrix.GetColumn(1).magnitude,
            matrix.GetColumn(2).magnitude
        );
        instance.transform.localScale = scale;

        if (ldrawColors.TryGetValue(colorCode, out Color color))
        {
            Renderer renderer = instance.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;
            }
        }
        else
        {
            Debug.LogWarning($"Mã màu LDraw {colorCode} không được định nghĩa.");
        }
    }

    private Vector3 ConvertToUnityCoordinates(Vector3 ldrawPosition)
    {
        // Chuyển đổi hệ tọa độ từ LDraw sang Unity (Y ngược dấu)
        return new Vector3(ldrawPosition.x, -ldrawPosition.y, ldrawPosition.z);
    }

    private Quaternion MatrixToQuaternion(Matrix4x4 m)
    {
        float trace = m.m00 + m.m11 + m.m22;
        Quaternion q = new Quaternion();

        if (trace > 0)
        {
            float s = Mathf.Sqrt(trace + 1.0f) * 2;
            q.w = 0.25f * s;
            q.x = (m.m21 - m.m12) / s;
            q.y = (m.m02 - m.m20) / s;
            q.z = (m.m10 - m.m01) / s;
        }
        else if (m.m00 > m.m11 && m.m00 > m.m22)
        {
            float s = Mathf.Sqrt(1.0f + m.m00 - m.m11 - m.m22) * 2;
            q.w = (m.m21 - m.m12) / s;
            q.x = 0.25f * s;
            q.y = (m.m01 + m.m10) / s;
            q.z = (m.m02 + m.m20) / s;
        }
        else if (m.m11 > m.m22)
        {
            float s = Mathf.Sqrt(1.0f + m.m11 - m.m00 - m.m22) * 2;
            q.w = (m.m02 - m.m20) / s;
            q.x = (m.m01 + m.m10) / s;
            q.y = 0.25f * s;
            q.z = (m.m12 + m.m21) / s;
        }
        else
        {
            float s = Mathf.Sqrt(1.0f + m.m22 - m.m00 - m.m11) * 2;
            q.w = (m.m10 - m.m01) / s;
            q.x = (m.m02 + m.m20) / s;
            q.y = (m.m12 + m.m21) / s;
            q.z = 0.25f * s;
        }

        return q;
    }
}