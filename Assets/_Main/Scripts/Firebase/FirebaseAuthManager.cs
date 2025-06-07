using UnityEngine;
using Firebase.Auth;
using Sirenix.OdinInspector;
using System.Threading.Tasks;
using System;
using Firebase;

public class FirebaseAuthManager : MonoBehaviour
{
    public static FirebaseAuthManager Instance { get; private set; }

    private string googleIdToken;

    private string googleAccessToken;

    private FirebaseAuth auth;
    private FirebaseUser currentUser;

    public bool IsSignedIn => currentUser != null;

    private async void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        await InitializeFirebase();
    }

    /// <summary>
    /// Initializes Firebase and checks dependencies.
    /// </summary>
    private async Task InitializeFirebase()
    {
        // Wait for Firebase to initialize
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == DependencyStatus.Available)
        {
            auth = FirebaseAuth.DefaultInstance;
            auth.StateChanged += AuthStateChanged;
            AuthStateChanged(this, null);
            Debug.Log("Firebase Authentication is ready!");
        }
        else
        {
            Debug.LogError($"Could not initialize Firebase: {dependencyStatus}");
        }
    }

    /// <summary>
    /// Handles authentication state changes.
    /// </summary>
    private async void AuthStateChanged(object sender, EventArgs eventArgs)
    {
        if (auth.CurrentUser != currentUser)
        {
            bool signedIn = currentUser != auth.CurrentUser && auth.CurrentUser != null && auth.CurrentUser.IsValid();
            if (!signedIn && currentUser != null)
            {
                Debug.Log($"Signed out: {currentUser.UserId}");
            }

            currentUser = auth.CurrentUser;

            if (signedIn)
            {
                Debug.Log($"Signed in: {currentUser.UserId}");
            }
        }
    }

    public string GetCurrentUserId()
    {
        return currentUser.UserId;
    }

    public bool GetCurrentUserVerified()
    {
        return currentUser.IsEmailVerified;
    }

    public async Task RegisterUser(string email, string password, string username)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogError("Email and password must not be empty!");
            return;
        }

        var result = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
        Debug.Log($"Registration successful: {result.User.Email}");
        // Automatically send verification email
        await result.User.SendEmailVerificationAsync();
        Debug.Log("Verification email sent!");
    }

    public async Task SignIn(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogError("Email and password must not be empty!");
            return;
        }
        await auth.SignInWithEmailAndPasswordAsync(email, password);
        //var result = await auth.SignInWithEmailAndPasswordAsync(email, password);
       // Debug.Log($"Sign in successful: {result.User.Email}");
    }

    private void SignOut()
    {
        if (auth != null)
        {
            auth.SignOut();
            Debug.Log("Signed out");
        }
    }

    private async void SendVerificationEmail()
    {
        if (currentUser != null && !currentUser.IsEmailVerified)
        {
            await currentUser.SendEmailVerificationAsync();
            Debug.Log("Verification email resent");
        }
        else
        {
            Debug.Log("Email already verified or user not signed in");
        }
    }

    public async Task ResetPassword(string email)
    {
        if (!string.IsNullOrEmpty(email))
        {
            await auth.SendPasswordResetEmailAsync(email);
            Debug.Log("Password reset email sent");
        }
        else
        {
            Debug.LogError("Please enter your email");
            throw new Exception("Please enter your email");
        }
    }

    private async Task SignInWithGoogle()
    {
        if (string.IsNullOrEmpty(googleIdToken) || string.IsNullOrEmpty(googleAccessToken))
        {
            Debug.LogError("Google ID Token and Access Token must not be empty!");
            return;
        }

        Credential credential = GoogleAuthProvider.GetCredential(googleIdToken, googleAccessToken);
        var result = await auth.SignInAndRetrieveDataWithCredentialAsync(credential);

        Debug.Log($"Google sign in successful: {result.User.DisplayName} ({result.User.UserId})");
    }

    public async Task SignInWithGooglePublic()
    {
        await SignInWithGoogle();
    }

    /// <summary>
    /// Returns a user-friendly error message for Firebase exceptions.
    /// </summary>
    private string GetFirebaseErrorMessage(FirebaseException ex)
    {
        switch (ex.ErrorCode)
        {
            case 17020: return "Invalid email";
            case 17026: return "Password is too weak";
            case 17008: return "Email format is incorrect";
            case 17007: return "Email is already in use";
            case 17009: return "Account already exists";
            case 17011: return "Email does not exist";
            case 17010: return "Incorrect password";
            case 17025: return "User is disabled";
            default: return ex.Message;
        }
    }

    private void OnDestroy()
    {
        if (auth != null)
        {
            auth.StateChanged -= AuthStateChanged;
            auth = null;
        }
    }
}
