using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector; // Thêm namespace của Odin

public class TestManager : MonoBehaviour
{
    [Header("Reference to View Info System")]
    [SerializeField] private DTNViewInfoSystem viewInfoSystem;

    [Header("Select View To Show")]
    [ValueDropdown("GetViewNames", IsUniqueList = true)]
    [SerializeField] private string viewNameToShow;

    // Hàm này trả về danh sách tên view cho dropdown
    private IEnumerable<string> GetViewNames()
    {
        if (viewInfoSystem == null || viewInfoSystem.ViewInfos == null)
            yield break;

        foreach (var info in viewInfoSystem.ViewInfos)
        {
            if (info != null && !string.IsNullOrEmpty(info.ViewName))
                yield return info.ViewName;
        }
    }

    private void Start()
    {
        if (viewInfoSystem == null)
        {
            Debug.LogError("ViewInfoSystem is not assigned!");
            return;
        }

        DTNWindow.FindTopWindow().ShowSubView(viewNameToShow);
    }
}