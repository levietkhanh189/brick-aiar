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

    // Model data model
    [FirestoreData]
    public class Model
    {
        [FirestoreProperty]
        public string ownerId { get; set; }

        [FirestoreProperty]
        public string title { get; set; }

        [FirestoreProperty]
        public string description { get; set; }

        [FirestoreProperty]
        public List<string> tags { get; set; }

        [FirestoreProperty]
        public string ldrUrl { get; set; }

        [FirestoreProperty]
        public string previewImageUrl { get; set; }

        [FirestoreProperty]
        public int brickCount { get; set; }

        [FirestoreProperty]
        public bool isPublic { get; set; }

        [FirestoreProperty]
        public int likes { get; set; }

        [FirestoreProperty]
        public int downloads { get; set; }

        [FirestoreProperty]
        public Timestamp createdAt { get; set; }

        [FirestoreProperty]
        public Timestamp updatedAt { get; set; }
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

    // Create or update model document
    public Task SetModelAsync(string modelId, Model model)
    {
        if (string.IsNullOrEmpty(modelId))
        {
            Debug.LogError("Model ID is null or empty.");
            return Task.CompletedTask;
        }

        DocumentReference docRef = db.Collection("models").Document(modelId);
        return docRef.SetAsync(model).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError($"Failed to set model document: {task.Exception}");
            }
            else
            {
                Debug.Log($"Model document {modelId} set successfully.");
            }
        });
    }

    // Get model document by id
    public Task<Model> GetModelAsync(string modelId)
    {
        DocumentReference docRef = db.Collection("models").Document(modelId);
        return docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError($"Failed to get model document: {task.Exception}");
                return null;
            }
            else
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    Model model = snapshot.ConvertTo<Model>();
                    return model;
                }
                else
                {
                    Debug.LogWarning($"Model document {modelId} does not exist.");
                    return null;
                }
            }
        });
    }

    // Example: Increment likes count atomically
    public Task IncrementModelLikesAsync(string modelId, int increment = 1)
    {
        DocumentReference docRef = db.Collection("models").Document(modelId);
        return docRef.UpdateAsync("likes", FieldValue.Increment(increment)).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError($"Failed to increment likes: {task.Exception}");
            }
            else
            {
                Debug.Log($"Likes incremented by {increment} for model {modelId}.");
            }
        });
    }

    // Example: Increment downloads count atomically
    public Task IncrementModelDownloadsAsync(string modelId, int increment = 1)
    {
        DocumentReference docRef = db.Collection("models").Document(modelId);
        return docRef.UpdateAsync("downloads", FieldValue.Increment(increment)).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError($"Failed to increment downloads: {task.Exception}");
            }
            else
            {
                Debug.Log($"Downloads incremented by {increment} for model {modelId}.");
            }
        });
    }
}
