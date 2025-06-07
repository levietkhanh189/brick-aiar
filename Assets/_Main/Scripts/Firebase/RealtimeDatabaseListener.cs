using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine; // Needed for Debug.Log
using System; // Needed for System.Serializable
using System.Collections.Generic;
using API;

public class RealtimeDatabaseListener : MonoBehaviour
{
    // Event để thông báo khi model hoàn thành
    public event Action<string, LegoModelData, string> OnModelCompleted;
    
    // Dictionary để lưu trữ các DatabaseReference đang listen
    private Dictionary<string, DatabaseReference> activeListeners = new Dictionary<string, DatabaseReference>();
    
    // Dictionary để lưu trữ các ValueEventHandler
    private Dictionary<string, EventHandler<ValueChangedEventArgs>> eventHandlers = new Dictionary<string, EventHandler<ValueChangedEventArgs>>();

    void Start()
    {
        // Khởi tạo Firebase Database nếu chưa có
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

    /// <summary>
    /// Bắt đầu listen cho một request cụ thể
    /// </summary>
    public void StartListeningForRequest(string requestId)
    {
        if (string.IsNullOrEmpty(requestId))
        {
            Debug.LogError("Request ID không được để trống!");
            return;
        }

        // Nếu đã đang listen cho request này rồi thì không cần listen lại
        if (activeListeners.ContainsKey(requestId))
        {
            Debug.Log($"Đã đang listen cho request: {requestId}");
            return;
        }

        try
        {
            /// Checkkkk lai phan nay

            // Lấy user_id từ FirebaseAuthManager
            string userId = FirebaseAuthManager.Instance.GetCurrentUserId();

            // Tạo path đến data trong Firebase: users/{user_id}/{request_id}
            string path = $"users/{userId}/{requestId}";
            DatabaseReference modelRef = FirebaseDatabase.DefaultInstance.GetReference(path);
            
            // Tạo event handler cho request này
            EventHandler<ValueChangedEventArgs> handler = (sender, args) => HandleValueChanged(requestId, sender, args);
            
            // Lưu trữ reference và handler
            activeListeners[requestId] = modelRef;
            eventHandlers[requestId] = handler;
            
            // Đăng ký listener
            modelRef.ValueChanged += handler;
            
            Debug.Log($"Bắt đầu listen cho request: {requestId} tại path: {path}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Lỗi khi bắt đầu listen cho request {requestId}: {e.Message}");
        }
    }

    /// <summary>
    /// Dừng listen cho một request cụ thể
    /// </summary>
    public void StopListeningForRequest(string requestId)
    {
        if (activeListeners.TryGetValue(requestId, out DatabaseReference modelRef) && 
            eventHandlers.TryGetValue(requestId, out EventHandler<ValueChangedEventArgs> handler))
        {
            modelRef.ValueChanged -= handler;
            activeListeners.Remove(requestId);
            eventHandlers.Remove(requestId);
            
            Debug.Log($"Đã dừng listen cho request: {requestId}");
        }
    }

    void HandleValueChanged(string requestId, object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError($"Lỗi khi lắng nghe thay đổi cho request {requestId}: {args.DatabaseError.Message}");
            OnModelCompleted?.Invoke(requestId, null, args.DatabaseError.Message);
            return;
        }

        DataSnapshot snapshot = args.Snapshot;
        if (snapshot.Exists && snapshot.Value != null)
        {
            string jsonString = snapshot.GetRawJsonValue();
            if (!string.IsNullOrEmpty(jsonString))
            {
                try
                {
                    LegoModelData model = JsonUtility.FromJson<LegoModelData>(jsonString);
                    
                    Debug.Log($"Nhận được update cho request {requestId}:");
                    Debug.Log($"Status: {model.status}");
                    Debug.Log($"Model URL: {model.model_url}");
                    
                    // Chỉ thông báo khi status là "success" hoặc "failed"
                    if (model.status == "success")
                    {
                        Debug.Log($"Request {requestId} hoàn thành thành công!");
                        OnModelCompleted?.Invoke(requestId, model, null);
                        
                        // Dừng listen sau khi hoàn thành
                        StopListeningForRequest(requestId);
                    }
                    else if (model.status == "failed")
                    {
                        Debug.LogError($"Request {requestId} thất bại!");
                        OnModelCompleted?.Invoke(requestId, null, "Request processing failed");
                        
                        // Dừng listen sau khi thất bại
                        StopListeningForRequest(requestId);
                    }
                    else if (model.status == "processing")
                    {
                        Debug.Log($"Request {requestId} đang được xử lý...");
                        // Không làm gì, tiếp tục chờ
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Lỗi khi giải mã JSON cho request {requestId}: {e.Message}");
                    OnModelCompleted?.Invoke(requestId, null, $"JSON parsing error: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"JSON rỗng cho request {requestId}");
            }
        }
        else
        {
            Debug.Log($"Không có dữ liệu cho request {requestId}");
        }
    }

    void OnDestroy()
    {
        // Cleanup tất cả listeners khi destroy
        foreach (var kvp in activeListeners)
        {
            if (eventHandlers.TryGetValue(kvp.Key, out EventHandler<ValueChangedEventArgs> handler))
            {
                kvp.Value.ValueChanged -= handler;
            }
        }
        
        activeListeners.Clear();
        eventHandlers.Clear();
        
        Debug.Log("RealtimeDatabaseListener đã cleanup tất cả listeners");
    }
}