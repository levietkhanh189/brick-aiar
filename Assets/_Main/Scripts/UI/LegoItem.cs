using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LegoItem : MonoBehaviour
{
    [Header("UI Components")]
    public Button buttonOpen;
    public TextMeshProUGUI nameText;
    public GameObject notify;

    [Header("Model Data")]
    public bool isNew;
    public string localLDRPath;
    
    // Thêm reference tới model data để dễ truy cập
    private API.LegoModelData modelData;

    /// <summary>
    /// Khởi tạo LegoItem với model data
    /// </summary>
    public void Initialize(API.LegoModelData data, string ldrPath)
    {
        modelData = data;
        localLDRPath = ldrPath;
        
        UpdateDisplay();
    }

    /// <summary>
    /// Cập nhật hiển thị UI
    /// </summary>
    public void UpdateDisplay()
    {
        if (modelData != null)
        {
            // Cập nhật tên
            if (nameText != null)
            {
                nameText.text = !string.IsNullOrEmpty(modelData.name) ? modelData.name : modelData.model_url;
            }

            // Cập nhật notify
            if (notify != null)
            {
                notify.SetActive(isNew);
            }
        }
    }

    /// <summary>
    /// Đánh dấu item là đã xem (không còn mới)
    /// </summary>
    public void MarkAsViewed()
    {
        if (isNew)
        {
            isNew = false;
            if (notify != null)
            {
                notify.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Lấy model data
    /// </summary>
    public API.LegoModelData GetModelData()
    {
        return modelData;
    }

    /// <summary>
    /// Kiểm tra xem item có data hợp lệ không
    /// </summary>
    public bool IsValid()
    {
        return modelData != null && !string.IsNullOrEmpty(localLDRPath);
    }

    /// <summary>
    /// Setup button click event
    /// </summary>
    public void SetupButton(System.Action<LegoItem> onClickCallback)
    {
        if (buttonOpen != null && onClickCallback != null)
        {
            buttonOpen.onClick.RemoveAllListeners();
            buttonOpen.onClick.AddListener(() => onClickCallback.Invoke(this));
        }
    }
}
