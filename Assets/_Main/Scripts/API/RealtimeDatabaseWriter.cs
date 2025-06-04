using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine; // Needed for Debug.Log
using System; // Needed for System.Serializable

public class RealtimeDatabaseWriter : MonoBehaviour
{
    private DatabaseReference databaseReference;

    void Start()
    {
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

        // Tạo dữ liệu mẫu giống JSON bạn gửi
        ModelData model = new ModelData(
            "naruto_20240519",
            "Right_test",
            "Naruto",
            "2025-05-19T14:03:00Z",
            "Mô hình Naruto tạo từ ảnh AI",
            0.01f,
            12550,
            "https://example.com/thumbnail/naruto.png",
            "https://example.com/models/naruto.ldr",
            "completed"
        );
        SaveModelData("models/naruto_20240519", model);
    }

    void SaveModelData(string path, ModelData model)
    {
        string json = JsonUtility.ToJson(model);
        databaseReference.Child(path).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                Debug.LogError("Lưu dữ liệu thất bại: " + task.Exception);
            }
            else
            {
                Debug.Log("Lưu dữ liệu thành công!");
            }
        });
    }
}