using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class FirestoreManagerTest : MonoBehaviour
{
    private FirestoreManager firestoreManager;

    private async void Start()
    {
        firestoreManager = GetComponent<FirestoreManager>();
        if (firestoreManager == null)
        {
            Debug.LogError("FirestoreManager component not found on the GameObject.");
            return;
        }

        await TestUserDocument();
        await TestModelDocument();
    }

    private async Task TestUserDocument()
    {
        Debug.Log("Starting User document test...");

        var user = new FirestoreManager.User
        {
            uid = "user_abc123",
            userName = "Nguyễn Văn A",
            email = "a@example.com",
            createdAt = Firebase.Firestore.Timestamp.FromDateTime(DateTime.UtcNow),
            role = "creator",
            modelCount = 12,
            isVerified = true
        };

        await firestoreManager.SetUserAsync(user);

        FirestoreManager.User fetchedUser = await firestoreManager.GetUserAsync(user.uid);
        if (fetchedUser != null)
        {
            Debug.Log($"User fetched: uid={fetchedUser.uid}, userName={fetchedUser.userName}, email={fetchedUser.email}, role={fetchedUser.role}, modelCount={fetchedUser.modelCount}, isVerified={fetchedUser.isVerified}");
        }
        else
        {
            Debug.LogError("Failed to fetch user document.");
        }
    }

    private async Task TestModelDocument()
    {
        Debug.Log("Starting Model document test...");

        string modelId = "model_xyz789";

        var model = new FirestoreManager.Model
        {
            ownerId = "user_abc123",
            title = "Robot Lắp Ráp Mini",
            description = "Mô hình robot đơn giản với 45 brick.",
            tags = new List<string> { "robot", "mini", "tech" },
            ldrUrl = "gs://your-app/models/xyz789.ldr",
            previewImageUrl = "https://example.com/preview.png",
            brickCount = 45,
            isPublic = true,
            likes = 132,
            downloads = 81,
            createdAt = Firebase.Firestore.Timestamp.FromDateTime(DateTime.UtcNow),
            updatedAt = Firebase.Firestore.Timestamp.FromDateTime(DateTime.UtcNow)
        };

        await firestoreManager.SetModelAsync(modelId, model);

        FirestoreManager.Model fetchedModel = await firestoreManager.GetModelAsync(modelId);
        if (fetchedModel != null)
        {
            Debug.Log($"Model fetched: id={modelId}, ownerId={fetchedModel.ownerId}, title={fetchedModel.title}, likes={fetchedModel.likes}, downloads={fetchedModel.downloads}");
        }
        else
        {
            Debug.LogError("Failed to fetch model document.");
        }

        // Test increment likes and downloads
        await firestoreManager.IncrementModelLikesAsync(modelId, 1);
        await firestoreManager.IncrementModelDownloadsAsync(modelId, 1);
    }
}
