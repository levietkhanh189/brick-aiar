using UnityEngine;
using System.Collections.Generic;
using System;
using API;

public class MainSceneManager : MonoBehaviour
{

    [Header("Settings")]
    [SerializeField] private bool autoLoadOnStart = true;
    [SerializeField] private bool autoDownloadLDRFiles = true;

    [Header("Debug Info")]
    [SerializeField] private int totalModelsLoaded = 0;
    [SerializeField] private int modelsWithLDR = 0;
    [SerializeField] private int pendingDownloads = 0;

    // Events cho UI hoặc các component khác - sử dụng API.LegoModelData
    public static event Action<API.LegoModelData, string> OnModelReady; // modelData, ldrFilePath
    public static event Action<string> OnModelError; // error message
    public static event Action<int, int> OnLoadingProgress; // loaded, total

    // Cache models - sử dụng API.LegoModelData
    private Dictionary<string, API.LegoModelData> loadedModels = new Dictionary<string, API.LegoModelData>();
    private Dictionary<string, string> modelLDRPaths = new Dictionary<string, string>();
    private HashSet<string> pendingDownloadModels = new HashSet<string>();

    void Start()
    {
        if (autoLoadOnStart)
        {
            InitializeAndLoadModels();
        }
    }

    void OnEnable()
    {
        SubscribeToEvents();
    }

    void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        // Subscribe to RealtimeDatabaseManager events
        RealtimeDatabaseManager.OnModelWithLDRReady += OnModelWithLDRReady;
        RealtimeDatabaseManager.OnNewModelDetected += OnNewModelDetected;
        RealtimeDatabaseManager.OnAllModelsLoaded += OnAllModelsLoaded;

        // Subscribe to LDRFileManager events
        LDRFileManager.OnLDRFileDownloaded += OnLDRFileDownloaded;
        LDRFileManager.OnLDRFileError += OnLDRFileError;
    }

    private void UnsubscribeFromEvents()
    {
        // Unsubscribe from RealtimeDatabaseManager events
        RealtimeDatabaseManager.OnModelWithLDRReady -= OnModelWithLDRReady;
        RealtimeDatabaseManager.OnNewModelDetected -= OnNewModelDetected;
        RealtimeDatabaseManager.OnAllModelsLoaded -= OnAllModelsLoaded;

        // Unsubscribe from LDRFileManager events
        LDRFileManager.OnLDRFileDownloaded -= OnLDRFileDownloaded;
        LDRFileManager.OnLDRFileError -= OnLDRFileError;
    }

    #region Event Handlers

    private void OnModelWithLDRReady(string modelId, LegoModelData modelData, string localLDRPath)
    {
        // Convert Firebase LegoModelData to API.LegoModelData
        var apiModelData = ConvertToAPIModelData(modelData);
        
        loadedModels[modelId] = apiModelData;
        modelLDRPaths[modelId] = localLDRPath;
        modelsWithLDR++;
        
        OnModelReady?.Invoke(apiModelData, localLDRPath);
        Debug.Log($"Model {modelId} sẵn sàng với LDR file: {localLDRPath}");
    }

    private void OnNewModelDetected(string modelId, LegoModelData modelData)
    {
        var apiModelData = ConvertToAPIModelData(modelData);
        loadedModels[modelId] = apiModelData;
        totalModelsLoaded++;
        
        OnLoadingProgress?.Invoke(totalModelsLoaded, totalModelsLoaded);
        Debug.Log($"Phát hiện model mới: {modelId}");
    }

    private void OnAllModelsLoaded(List<LegoModelData> models)
    {
        if (models != null)
        {
            totalModelsLoaded = models.Count;
            OnLoadingProgress?.Invoke(totalModelsLoaded, totalModelsLoaded);
            Debug.Log($"Đã tải tất cả {totalModelsLoaded} models");
        }
    }

    private void OnLDRFileDownloaded(string modelId, string filePath)
    {
        modelLDRPaths[modelId] = filePath;
        pendingDownloadModels.Remove(modelId);
        pendingDownloads = pendingDownloadModels.Count;
        
        if (loadedModels.ContainsKey(modelId))
        {
            OnModelReady?.Invoke(loadedModels[modelId], filePath);
        }
        
        Debug.Log($"LDR file tải xong cho model {modelId}: {filePath}");
    }

    private void OnLDRFileError(string modelId, string error)
    {
        pendingDownloadModels.Remove(modelId);
        pendingDownloads = pendingDownloadModels.Count;
        OnModelError?.Invoke($"Lỗi tải LDR cho model {modelId}: {error}");
        Debug.LogError($"Lỗi tải LDR cho model {modelId}: {error}");
    }

    private API.LegoModelData ConvertToAPIModelData(LegoModelData firebaseData)
    {
        return new API.LegoModelData
        {
            requestId = firebaseData.user_id, // Sử dụng user_id làm requestId tạm thời
            user_id = firebaseData.user_id,
            category = firebaseData.category,
            created_at = float.Parse(firebaseData.created_at),
            description = firebaseData.description,
            model_url = firebaseData.model_url,
            name = firebaseData.name,
            polycount = firebaseData.poly_count.ToString(),
            price = firebaseData.price,
            status = firebaseData.status,
            thumbnail_url = firebaseData.thumbnail_url
        };
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Khởi tạo và tải tất cả models
    /// </summary>
    public void InitializeAndLoadModels()
    {
        Debug.Log("Bắt đầu khởi tạo và tải models...");

        // Reset counters
        totalModelsLoaded = 0;
        modelsWithLDR = 0;
        pendingDownloads = 0;

        loadedModels.Clear();
        modelLDRPaths.Clear();
        pendingDownloadModels.Clear();

        // Bắt đầu đọc models từ Firebase
        RealtimeDatabaseManager.Instance.ReadAllModelsForCurrentUser();
    }

    /// <summary>
    /// Lấy model data theo ID
    /// </summary>
    public API.LegoModelData GetModel(string modelId)
    {
        return loadedModels.ContainsKey(modelId) ? loadedModels[modelId] : null;
    }

    /// <summary>
    /// Lấy đường dẫn LDR file của model
    /// </summary>
    public string GetModelLDRPath(string modelId)
    {
        return modelLDRPaths.ContainsKey(modelId) ? modelLDRPaths[modelId] : null;
    }

    /// <summary>
    /// Lấy tất cả models đã tải
    /// </summary>
    public List<API.LegoModelData> GetAllModels()
    {
        return new List<API.LegoModelData>(loadedModels.Values);
    }

    /// <summary>
    /// Lấy danh sách models đã có LDR file
    /// </summary>
    public List<string> GetModelsWithLDR()
    {
        return new List<string>(modelLDRPaths.Keys);
    }

    /// <summary>
    /// Lấy thông tin tổng quan về inventory
    /// </summary>
    public InventoryInfo GetInventoryInfo()
    {
        return new InventoryInfo
        {
            totalModels = totalModelsLoaded,
            modelsWithLDR = modelsWithLDR,
            pendingDownloads = pendingDownloads,
            loadedModels = loadedModels.Count
        };
    }

    /// <summary>
    /// Kiểm tra xem có model mới không (được tạo trong 24h qua)
    /// </summary>
    public bool HasNewModels()
    {
        float currentTime = Time.time;
        foreach (var model in loadedModels.Values)
        {
            float timeDifference = currentTime - model.created_at;
            if (timeDifference < 86400f) // 24 hours
            {
                return true;
            }
        }
        return false;
    }

    #endregion
}

/// <summary>
/// Struct chứa thông tin tổng quan về inventory
/// </summary>
[System.Serializable]
public struct InventoryInfo
{
    public int totalModels;
    public int modelsWithLDR;
    public int pendingDownloads;
    public int loadedModels;
}
