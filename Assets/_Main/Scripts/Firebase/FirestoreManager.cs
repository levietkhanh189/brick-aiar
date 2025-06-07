using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;

public class FirestoreManager : MonoBehaviour
{
    private FirebaseFirestore db;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        InitializeFirestore();
    }

    private void InitializeFirestore()
    {
        db = FirebaseFirestore.DefaultInstance;
        Debug.Log("Firestore initialized.");
    }

    // User data model
    [FirestoreData]
    public class User
    {
        [FirestoreProperty]
        public string uid { get; set; }

        [FirestoreProperty]
        public string userName { get; set; }

        [FirestoreProperty]
        public string email { get; set; }

        [FirestoreProperty]
        public Timestamp createdAt { get; set; }

        [FirestoreProperty]
        public string role { get; set; }

        [FirestoreProperty]
        public int modelCount { get; set; }

        [FirestoreProperty]
        public bool isVerified { get; set; }
    }

    // Create or update user document
    public Task SetUserAsync(User user)
    {
        if (string.IsNullOrEmpty(user.uid))
        {
            Debug.LogError("User UID is null or empty.");
            return Task.CompletedTask;
        }

        DocumentReference docRef = db.Collection("users").Document(user.uid);
        return docRef.SetAsync(user).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError($"Failed to set user document: {task.Exception}");
            }
            else
            {
                Debug.Log($"User document {user.uid} set successfully.");
            }
        });
    }

    // Get user document by uid
    public Task<User> GetUserAsync(string uid)
    {
        DocumentReference docRef = db.Collection("users").Document(uid);
        return docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError($"Failed to get user document: {task.Exception}");
                return null;
            }
            else
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    User user = snapshot.ConvertTo<User>();
                    return user;
                }
                else
                {
                    Debug.LogWarning($"User document {uid} does not exist.");
                    return null;
                }
            }
        });
    }
}
