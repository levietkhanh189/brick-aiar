using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static FirestoreManager; // For User and Model classes

/// <summary>
/// DataController provides high-level CRUD operations for User and Model data
/// using FirestoreManager as the backend.
/// </summary>
[RequireComponent(typeof(FirestoreManager))]
public class DataController : MonoBehaviour
{
    private FirestoreManager firestoreManager;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        firestoreManager = GetComponent<FirestoreManager>();
        if (firestoreManager == null)
        {
            Debug.LogError("FirestoreManager component not found on DataController GameObject.");
        }
    }

    #region User CRUD

    /// <summary>
    /// Create or update a user in Firestore.
    /// </summary>
    public async Task CreateOrUpdateUserAsync(User user)
    {
        if (firestoreManager == null)
        {
            Debug.LogError("FirestoreManager is not initialized.");
            return;
        }
        try
        {
            await firestoreManager.SetUserAsync(user);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    /// <summary>
    /// Load user info by UID.
    /// </summary>
    public async Task<User> LoadUserAsync(string uid)
    {
        if (firestoreManager == null)
        {
            Debug.LogError("FirestoreManager is not initialized.");
            return null;
        }
        try
        {
            return await firestoreManager.GetUserAsync(uid);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return null;
        }
    }

    #endregion

    #region Model CRUD

    /// <summary>
    /// Create or update a model in Firestore.
    /// </summary>
    public async Task CreateOrUpdateModelAsync(string modelId, Model model)
    {
        if (firestoreManager == null)
        {
            Debug.LogError("FirestoreManager is not initialized.");
            return;
        }
        try
        {
            await firestoreManager.SetModelAsync(modelId, model);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    /// <summary>
    /// Load model info by modelId.
    /// </summary>
    public async Task<Model> LoadModelAsync(string modelId)
    {
        if (firestoreManager == null)
        {
            Debug.LogError("FirestoreManager is not initialized.");
            return null;
        }
        try
        {
            return await firestoreManager.GetModelAsync(modelId);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return null;
        }
    }

    /// <summary>
    /// Increment the likes count for a model.
    /// </summary>
    public async Task IncrementModelLikesAsync(string modelId, int increment = 1)
    {
        if (firestoreManager == null)
        {
            Debug.LogError("FirestoreManager is not initialized.");
            return;
        }
        try
        {
            await firestoreManager.IncrementModelLikesAsync(modelId, increment);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    /// <summary>
    /// Increment the downloads count for a model.
    /// </summary>
    public async Task IncrementModelDownloadsAsync(string modelId, int increment = 1)
    {
        if (firestoreManager == null)
        {
            Debug.LogError("FirestoreManager is not initialized.");
            return;
        }
        try
        {
            await firestoreManager.IncrementModelDownloadsAsync(modelId, increment);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    #endregion

    // Additional CRUD methods (Delete, List, etc.) can be added as needed.
}
