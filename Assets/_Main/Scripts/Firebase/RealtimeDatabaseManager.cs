using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using System;
using System.Collections.Generic;
using API;

[System.Serializable]
public class ModelData
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

public class RealtimeDatabaseManager: MonoBehaviour
{
    public static RealtimeDatabaseManager Instance { get; private set; }

    // Reference cho listener ChildAdded trên userId
    private DatabaseReference userModelsListenerRef;
    private EventHandler<ChildChangedEventArgs> userModelsChildAddedHandler;

    // HashSet để theo dõi các model đã được load trong lần đầu
    private HashSet<string> initialModels = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        InitializeFirebaseDatabase();
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

    [Sirenix.OdinInspector.Button]
    public void ReadAllModelsForCurrentUser(Action<string, LegoModelData> OnModelCompleted, Action<List<LegoModelData>> OnAllModelsLoaded)
    {
        string userId = FirebaseAuthManager.Instance.GetCurrentUserId();
        string path = $"{userId}";
        DatabaseReference userRef = FirebaseDatabase.DefaultInstance.GetReference(path);

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

                                    Debug.Log($"Đã load model: {modelKey}");

                                    // Fire OnModelCompleted cho mỗi model đã có
                                    OnModelCompleted?.Invoke(model.model_url, model);
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
            }
        });
    }

    public void StartListeningForNewModels(Action<string, LegoModelData> OnModelCompleted)
    {
        string userId = FirebaseAuthManager.Instance.GetCurrentUserId();
        string path = $"{userId}";
        userModelsListenerRef = FirebaseDatabase.DefaultInstance.GetReference(path);

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
                        Debug.Log($"Model mới được thêm: {modelKey}");
                        OnModelCompleted?.Invoke(model.model_url, model);
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
    }
}