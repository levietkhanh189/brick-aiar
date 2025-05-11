using UnityEngine;
using System;

/// <summary>
/// Singleton class to store and manage the current user's information.
/// </summary>
public class UserInfo : MonoBehaviour
{
    private static UserInfo _instance;
    
    public static UserInfo Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("UserInfo");
                _instance = go.AddComponent<UserInfo>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    // User data properties
    public string Uid { get; private set; }
    public string UserName { get; private set; }
    public string Email { get; private set; }
    public string Role { get; private set; }
    public int ModelCount { get; private set; }
    public bool IsVerified { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    // Event to notify when user data changes
    public event Action OnUserDataChanged;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Set the current user's information.
    /// </summary>
    public void SetUserData(string uid, string userName, string email, DateTime createdAt, string role = "user", int modelCount = 0, bool isVerified = false)
    {
        Uid = uid;
        UserName = userName;
        Email = email;
        CreatedAt = createdAt;
        Role = role;
        ModelCount = modelCount;
        IsVerified = isVerified;
        
        // Notify listeners that user data has changed
        OnUserDataChanged?.Invoke();
        
        Debug.Log($"User data set: {UserName} ({Uid})");
    }

    /// <summary>
    /// Set the current user's information from a FirestoreManager.User object.
    /// </summary>
    public void SetUserData(FirestoreManager.User user)
    {
        if (user == null)
        {
            Debug.LogError("Cannot set user data from null user object");
            return;
        }
        
        Uid = user.uid;
        UserName = user.userName;
        Email = user.email;
        CreatedAt = user.createdAt != null ? user.createdAt.ToDateTime() : DateTime.Now;
        Role = user.role;
        ModelCount = user.modelCount;
        IsVerified = user.isVerified;
        
        // Notify listeners that user data has changed
        OnUserDataChanged?.Invoke();
        
        Debug.Log($"User data set from Firestore: {UserName} ({Uid})");
    }

    [Sirenix.OdinInspector.Button]
    public void DebugUserData()
    {
        Debug.Log($"User data: {UserName} ({Uid})");
        Debug.Log($"Email: {Email}");
        Debug.Log($"Created at: {CreatedAt}");
        Debug.Log($"Role: {Role}");
        Debug.Log($"Model count: {ModelCount}");
        Debug.Log($"Is verified: {IsVerified}");    
    }

    /// <summary>
    /// Clear the current user's information (e.g., on logout).
    /// </summary>
    public void ClearUserData()
    {
        Uid = null;
        UserName = null;
        Email = null;
        CreatedAt = default;
        Role = null;
        ModelCount = 0;
        IsVerified = false;
        
        // Notify listeners that user data has changed
        OnUserDataChanged?.Invoke();
        
        Debug.Log("User data cleared");
    }

    /// <summary>
    /// Check if a user is currently logged in.
    /// </summary>
    public bool IsLoggedIn()
    {
        return !string.IsNullOrEmpty(Uid);
    }

    /// <summary>
    /// Update the model count for the current user.
    /// </summary>
    public void UpdateModelCount(int count)
    {
        ModelCount = count;
        OnUserDataChanged?.Invoke();
    }

    /// <summary>
    /// Update the verification status for the current user.
    /// </summary>
    public void UpdateVerificationStatus(bool isVerified)
    {
        IsVerified = isVerified;
        OnUserDataChanged?.Invoke();
    }
}
