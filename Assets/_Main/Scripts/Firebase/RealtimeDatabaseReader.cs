using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine; // Needed for Debug.Log
using System; // Needed for System.Serializable

// Assuming ModelData class is defined as above

[System.Serializable]
public class ModelData
{
    public string requestId;
    public string user_id;
    public string name;
    public string created_at;
    public string description;
    public float details;
    public int piece_count;
    public string thumbnail_url;
    public string ldr_url;
    public string status;

    // Thêm các trường khác nếu cần

    public ModelData() { }

    public ModelData(
        string requestId,
        string user_id,
        string name,
        string created_at,
        string description,
        float details,
        int piece_count,
        string thumbnail_url,
        string ldr_url,
        string status
    )
    {
        this.requestId = requestId;
        this.user_id = user_id;
        this.name = name;
        this.created_at = created_at;
        this.description = description;
        this.details = details;
        this.piece_count = piece_count;
        this.thumbnail_url = thumbnail_url;
        this.ldr_url = ldr_url;
        this.status = status;
    }
}

public class RealtimeDatabaseReader : MonoBehaviour
{
    private DatabaseReference databaseReference;

    void Start()
    {
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        ReadModelData("models/naruto_20240519");
    }

    void ReadModelData(string path)
    {
        databaseReference.Child(path).GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                Debug.LogError("Đọc dữ liệu thất bại: " + task.Exception);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists && snapshot.Value != null)
                {
                    string jsonString = snapshot.GetRawJsonValue();
                    if (!string.IsNullOrEmpty(jsonString))
                    {
                        try
                        {
                            ModelData model = JsonUtility.FromJson<ModelData>(jsonString);
                            Debug.Log("Đọc dữ liệu thành công:");
                            Debug.Log("Request ID: " + model.requestId);
                            Debug.Log("User ID: " + model.user_id);
                            Debug.Log("Name: " + model.name);
                            Debug.Log("Created At: " + model.created_at);
                            Debug.Log("Description: " + model.description);
                            Debug.Log("Details: " + model.details);
                            Debug.Log("Piece Count: " + model.piece_count);
                            Debug.Log("Thumbnail URL: " + model.thumbnail_url);
                            Debug.Log("LDR URL: " + model.ldr_url);
                            Debug.Log("Status: " + model.status);
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError("Lỗi khi giải mã JSON: " + e.Message);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("JSON rỗng hoặc null tại path: " + path);
                    }
                }
                else
                {
                    Debug.Log("Không tìm thấy dữ liệu tại path: " + path);
                }
            }
        });
    }

    // You could do the same deserialization within the ValueChanged handler as well!
    // Just get the snapshot from args.Snapshot and deserialize it.
}