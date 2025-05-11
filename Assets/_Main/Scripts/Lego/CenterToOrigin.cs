using UnityEngine;
using UnityEditor;

public class CenterToOrigin: MonoBehaviour
{
    [MenuItem("LDraw Tools/Center and Flip Y (to Right-Handed)")]
    static void CenterAndFlipY()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            Debug.LogWarning("Vui lòng chọn một GameObject.");
            return;
        }

        // Tính toán bounding box
        Bounds bounds = GetBounds(selected);
        Vector3 center = bounds.center;

        // Tạo parent object tại center
        GameObject parent = new GameObject(selected.name + "_LDrawCentered");
        parent.transform.position = center;

        // Di chuyển đối tượng thành con
        selected.transform.SetParent(parent.transform);
        selected.transform.localPosition -= center - parent.transform.position;

        // Flip trục Y (giống LDraw: -Y là hướng lên)
        parent.transform.localScale = new Vector3(1, -1, 1);  // Lật theo trục Y

        Debug.Log($"[LDraw] Mô hình đã được căn giữa tại {center} và chuyển sang hệ tọa độ tay phải (Y âm).");
    }

    static Bounds GetBounds(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        Collider[] colliders = go.GetComponentsInChildren<Collider>();
        MeshFilter[] meshFilters = go.GetComponentsInChildren<MeshFilter>();

        Bounds bounds = new Bounds(go.transform.position, Vector3.zero);
        bool hasBounds = false;

        if (renderers.Length > 0)
        {
            bounds = renderers[0].bounds;
            foreach (var r in renderers)
                bounds.Encapsulate(r.bounds);
            hasBounds = true;
        }
        else if (colliders.Length > 0)
        {
            bounds = colliders[0].bounds;
            foreach (var c in colliders)
                bounds.Encapsulate(c.bounds);
            hasBounds = true;
        }
        else if (meshFilters.Length > 0)
        {
            foreach (var mf in meshFilters)
            {
                if (mf.sharedMesh == null) continue;
                Bounds b = mf.sharedMesh.bounds;
                Vector3 worldCenter = mf.transform.TransformPoint(b.center);
                Bounds worldBounds = new Bounds(worldCenter, mf.transform.TransformVector(b.size));
                if (!hasBounds)
                {
                    bounds = worldBounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(worldBounds);
                }
            }
        }

        return bounds;
    }
}
