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
    public static DataController Instance;
    private FirestoreManager firestoreManager;

    private void Awake()
    {
        Instance = this;
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
    public async Task LoadUserAsync()
    {
        FirestoreManager.User user = await DataController.Instance.LoadUserAsync(FirebaseAuthManager.Instance.GetCurrentUserId());
        UserInfo.Instance.SetUserData(user);
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
}
