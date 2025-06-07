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
}
