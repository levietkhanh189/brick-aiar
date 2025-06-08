using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using System;
using System.Collections.Generic;
using API;

[System.Serializable]
public class LegoModelData
{
    public string category;
    public string created_at;
    public string description;
    public string model_url;
    public string name;
    public int poly_count;
    public string price;
    public string status;
    public string thumbnail_url;
    public string user_id;
}

public class RealtimeDatabaseManager : MonoBehaviour
{
    public static RealtimeDatabaseManager Instance { get; private set; }

    // Reference cho listener ChildAdded trên userId
    private DatabaseReference userModelsListenerRef;
    private EventHandler<ChildChangedEventArgs> userModelsChildAddedHandler;

    // HashSet để theo dõi các model đã được load trong lần đầu
    private HashSet<string> initialModels = new HashSet<string>();

    // Events để thông báo khi có model mới
    public static event Action<string, LegoModelData, string> OnModelWithLDRReady; // modelId, modelData, localLDRPath
    public static event Action<string, LegoModelData> OnNewModelDetected; // modelId, modelData
    public static event Action<List<LegoModelData>> OnAllModelsLoaded;

    // Dictionary để lưu model data theo modelId
    private Dictionary<string, LegoModelData> modelDataCache = new Dictionary<string, LegoModelData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFirebaseDatabase();
            SubscribeToLDREvents();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeFirebaseDatabase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                Debug.Log("Firebase Database is ready for listening!");
            }
            else
            {
                Debug.LogError($"Could not resolve Firebase dependencies: {dependencyStatus}");
            }
        });
    }

    private void SubscribeToLDREvents()
    {
        // Lắng nghe events từ LDRFileManager
        LDRFileManager.OnLDRFileDownloaded += OnLDRFileDownloaded;
        LDRFileManager.OnLDRFileError += OnLDRFileError;
    }

    private void OnLDRFileDownloaded(string modelId, string filePath)
    {
        Debug.Log($"LDR file đã sẵn sàng cho model {modelId}: {filePath}");

        // Lấy model data từ cache
        if (modelDataCache.ContainsKey(modelId))
        {
            var modelData = modelDataCache[modelId];
            OnModelWithLDRReady?.Invoke(modelId, modelData, filePath);
        }
    }

    private void OnLDRFileError(string modelId, string error)
    {
        Debug.LogError($"Lỗi tải LDR file cho model {modelId}: {error}");
    }

    [Sirenix.OdinInspector.Button]
    public void ReadAllModelsForCurrentUser()
    {
        string userId = FirebaseAuthManager.Instance.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID không hợp lệ!");
            return;
        }

        string path = $"{userId}";
        DatabaseReference userRef = FirebaseDatabase.DefaultInstance.GetReference(path);

        Debug.Log($"Đang đọc tất cả models cho user: {userId}");

        userRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Đọc tất cả models thất bại: " + task.Exception);
                OnAllModelsLoaded?.Invoke(null);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                List<LegoModelData> models = new List<LegoModelData>();
                initialModels.Clear();
                modelDataCache.Clear();

                if (snapshot.Exists && snapshot.HasChildren)
                {
                    foreach (var child in snapshot.Children)
                    {
                        string modelKey = child.Key;

                        if (modelKey.StartsWith("model_"))
                        {
                            initialModels.Add(modelKey);

                            string jsonString = child.GetRawJsonValue();
                            if (!string.IsNullOrEmpty(jsonString))
                            {
                                try
                                {
                                    LegoModelData model = JsonUtility.FromJson<LegoModelData>(jsonString);
                                    models.Add(model);
                                    modelDataCache[modelKey] = model;

                                    Debug.Log($"Đã load model: {modelKey}");

                                    // Kiểm tra xem LDR file đã có trong cache chưa
                                    CheckAndDownloadLDRFile(modelKey, userId, model);
                                }
                                catch (Exception e)
                                {
                                    Debug.LogError($"Lỗi khi giải mã JSON cho model {modelKey}: {e.Message}");
                                }
                            }
                        }
                    }
                }

                Debug.Log($"Đã load {models.Count} models từ database");
                OnAllModelsLoaded?.Invoke(models);

                // Bắt đầu listen cho models mới sau khi load xong
                StartListeningForNewModels();
            }
        });
    }

    public void StartListeningForNewModels()
    {
        string userId = FirebaseAuthManager.Instance.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID không hợp lệ!");
            return;
        }

        string path = $"{userId}";
        userModelsListenerRef = FirebaseDatabase.DefaultInstance.GetReference(path);

        // Hủy listener cũ nếu có
        if (userModelsChildAddedHandler != null)
        {
            userModelsListenerRef.ChildAdded -= userModelsChildAddedHandler;
        }

        userModelsChildAddedHandler = (object sender, ChildChangedEventArgs args) =>
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError($"Lỗi khi listen ChildAdded: {args.DatabaseError.Message}");
                return;
            }

            if (args.Snapshot != null && args.Snapshot.Exists && args.Snapshot.Value != null)
            {
                string modelKey = args.Snapshot.Key;

                if (!modelKey.StartsWith("model_"))
                {
                    return;
                }

                // Bỏ qua models đã có từ lần đọc đầu tiên
                if (initialModels.Contains(modelKey))
                {
                    return;
                }

                string jsonString = args.Snapshot.GetRawJsonValue();

                if (!string.IsNullOrEmpty(jsonString))
                {
                    try
                    {
                        LegoModelData model = JsonUtility.FromJson<LegoModelData>(jsonString);
                        modelDataCache[modelKey] = model;

                        Debug.Log($"Model mới được phát hiện: {modelKey}");
                        OnNewModelDetected?.Invoke(modelKey, model);

                        // Tự động tải LDR file cho model mới
                        CheckAndDownloadLDRFile(modelKey, userId, model);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Lỗi khi giải mã JSON cho model mới {modelKey}: {e.Message}");
                    }
                }
            }
        };

        userModelsListenerRef.ChildAdded += userModelsChildAddedHandler;
        Debug.Log($"Bắt đầu listen ChildAdded cho user: {userId}");
    }

    private void CheckAndDownloadLDRFile(string modelId, string userId, LegoModelData modelData)
    {
        // Kiểm tra xem LDR file đã có trong cache chưa
        if (LDRFileManager.Instance.IsModelCached(modelId))
        {
            string cachedPath = LDRFileManager.Instance.GetLocalFilePath(modelId);
            Debug.Log($"Model {modelId} đã có LDR file trong cache: {cachedPath}");
            OnModelWithLDRReady?.Invoke(modelId, modelData, cachedPath);
        }
        else
        {
            // Tải LDR file mới
            Debug.Log($"Bắt đầu tải LDR file cho model {modelId}");
            LDRFileManager.Instance.GetLDRFileByModelId(modelId, userId, modelData.model_url);
        }
    }

    // Lấy model data theo modelId
    public LegoModelData GetModelData(string modelId)
    {
        return modelDataCache.ContainsKey(modelId) ? modelDataCache[modelId] : null;
    }

    // Lấy tất cả model data đã cache
    public List<LegoModelData> GetAllCachedModels()
    {
        return new List<LegoModelData>(modelDataCache.Values);
    }

    // Lấy model cùng với LDR file path (nếu có)
    public void GetModelWithLDR(string modelId, Action<LegoModelData, string> onComplete)
    {
        var modelData = GetModelData(modelId);
        if (modelData == null)
        {
            onComplete?.Invoke(null, null);
            return;
        }

        string ldrPath = LDRFileManager.Instance.GetLocalFilePath(modelId);
        if (!string.IsNullOrEmpty(ldrPath))
        {
            onComplete?.Invoke(modelData, ldrPath);
        }
        else
        {
            // Tải LDR file nếu chưa có
            string userId = FirebaseAuthManager.Instance.GetCurrentUserId();
            LDRFileManager.Instance.GetLDRFileByModelId(modelId, userId, modelData.model_url,
                (filePath, error) =>
                {
                    onComplete?.Invoke(modelData, filePath);
                });
        }
    }

    // Force reload một model cụ thể
    public void ReloadModel(string modelId)
    {
        string userId = FirebaseAuthManager.Instance.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID không hợp lệ!");
            return;
        }

        string path = $"{userId}/{modelId}";
        DatabaseReference modelRef = FirebaseDatabase.DefaultInstance.GetReference(path);

        modelRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"Lỗi reload model {modelId}: " + task.Exception);
            }
            else if (task.IsCompleted && task.Result.Exists)
            {
                string jsonString = task.Result.GetRawJsonValue();
                if (!string.IsNullOrEmpty(jsonString))
                {
                    try
                    {
                        LegoModelData model = JsonUtility.FromJson<LegoModelData>(jsonString);
                        modelDataCache[modelId] = model;

                        Debug.Log($"Đã reload model: {modelId}");
                        CheckAndDownloadLDRFile(modelId, userId, model);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Lỗi parse model {modelId}: {e.Message}");
                    }
                }
            }
        });
    }

    public void StopListeningForNewModels()
    {
        if (userModelsListenerRef != null && userModelsChildAddedHandler != null)
        {
            userModelsListenerRef.ChildAdded -= userModelsChildAddedHandler;
            userModelsListenerRef = null;
            userModelsChildAddedHandler = null;
            Debug.Log("Đã dừng listen ChildAdded cho user");
        }
    }

    void OnDestroy()
    {
        StopListeningForNewModels();

        // Unsubscribe from LDR events
        LDRFileManager.OnLDRFileDownloaded -= OnLDRFileDownloaded;
        LDRFileManager.OnLDRFileError -= OnLDRFileError;
    }
}