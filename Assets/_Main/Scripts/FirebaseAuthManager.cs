using UnityEngine;
using Firebase.Auth;
using Sirenix.OdinInspector;
using System.Threading.Tasks;
using System;
using Firebase;

public class FirebaseAuthManager : MonoBehaviour
{
    [TabGroup("Login Info")]
    [SerializeField] 
    private string email;
    
    [TabGroup("Login Info")]
    [SerializeField] 
    private string password;

    [TabGroup("Login Info")]
    [SerializeField]
    private string googleIdToken;

    [TabGroup("Login Info")]
    [SerializeField]
    private string googleAccessToken;

    [FoldoutGroup("User Info", expanded: false)]
    [ReadOnly, ShowInInspector]
    private string userId;

    [FoldoutGroup("User Info")]
    [ReadOnly, ShowInInspector]
    private string displayName;

    [FoldoutGroup("User Info")]
    [ReadOnly, ShowInInspector]
    private string userEmail;

    [FoldoutGroup("User Info")]
    [ReadOnly, ShowInInspector]
    private bool isEmailVerified;

    private FirebaseAuth auth;
    private FirebaseUser currentUser;

    [BoxGroup("Status")]
    [ShowInInspector, ReadOnly]
    public bool IsSignedIn => currentUser != null;

    private async void Start()
    {
        await InitializeFirebase();
    }

    /// <summary>
    /// Initializes Firebase and checks dependencies.
    /// </summary>
    private async Task InitializeFirebase()
    {
        try
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
        catch (Exception ex)
        {
            Debug.LogError($"Firebase initialization error: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles authentication state changes.
    /// </summary>
    private void AuthStateChanged(object sender, EventArgs eventArgs)
    {
        if (auth.CurrentUser != currentUser)
        {
            bool signedIn = currentUser != auth.CurrentUser && auth.CurrentUser != null && auth.CurrentUser.IsValid();
            if (!signedIn && currentUser != null)
            {
                Debug.Log($"Signed out: {currentUser.UserId}");
                ClearUserInfo();
            }

            currentUser = auth.CurrentUser;
            if (signedIn)
            {
                UpdateUserInfo();
                Debug.Log($"Signed in: {currentUser.UserId}");
            }
        }
    }

    /// <summary>
    /// Updates user information fields from the current user.
    /// </summary>
    private void UpdateUserInfo()
    {
        if (currentUser != null)
        {
            userId = currentUser.UserId;
            displayName = currentUser.DisplayName ?? "No display name";
            userEmail = currentUser.Email;
            isEmailVerified = currentUser.IsEmailVerified;
        }
    }

    /// <summary>
    /// Clears user information fields.
    /// </summary>
    private void ClearUserInfo()
    {
        userId = string.Empty;
        displayName = string.Empty;
        userEmail = string.Empty;
        isEmailVerified = false;
    }

    [TabGroup("Actions")]
    [Button("Register", ButtonSizes.Large)]
    public async Task RegisterUser()
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogError("Email and password must not be empty!");
            return;
        }

        try
        {
            var result = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            Debug.Log($"Registration successful: {result.User.Email}");
            // Automatically send verification email
            await result.User.SendEmailVerificationAsync();
            Debug.Log("Verification email sent!");
        }
        catch (FirebaseException ex)
        {
            string errorMessage = GetFirebaseErrorMessage(ex);
            Debug.LogError($"Registration error: {errorMessage}");
            throw new Exception(errorMessage);
        }
    }

    [TabGroup("Actions")]
    [Button("Sign In", ButtonSizes.Large)]
    public async Task SignIn()
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogError("Email and password must not be empty!");
            return;
        }

        try
        {
            var result = await auth.SignInWithEmailAndPasswordAsync(email, password);
            Debug.Log($"Sign in successful: {result.User.Email}");
        }
        catch (FirebaseException ex)
        {
            string errorMessage = GetFirebaseErrorMessage(ex);
            Debug.LogError($"Sign in error: {errorMessage}");
            throw new Exception(errorMessage);
        }
    }

    [TabGroup("Actions")]
    [Button("Sign Out", ButtonSizes.Large)]
    [EnableIf("IsSignedIn")]
    private void SignOut()
    {
        if (auth != null)
        {
            auth.SignOut();
            Debug.Log("Signed out");
        }
    }

    [TabGroup("Actions")]
    [Button("Resend Verification Email", ButtonSizes.Large)]
    [EnableIf("IsSignedIn")]
    private async void SendVerificationEmail()
    {
        if (currentUser != null && !currentUser.IsEmailVerified)
        {
            try
            {
                await currentUser.SendEmailVerificationAsync();
                Debug.Log("Verification email resent");
            }
            catch (FirebaseException ex)
            {
                string errorMessage = GetFirebaseErrorMessage(ex);
                Debug.LogError($"Verification email error: {errorMessage}");
            }
        }
        else
        {
            Debug.Log("Email already verified or user not signed in");
        }
    }

    [TabGroup("Actions")]
    [Button("Reset Password", ButtonSizes.Large)]
    public async Task ResetPassword()
    {
        if (!string.IsNullOrEmpty(email))
        {
            try
            {
                await auth.SendPasswordResetEmailAsync(email);
                Debug.Log("Password reset email sent");
            }
            catch (FirebaseException ex)
            {
                string errorMessage = GetFirebaseErrorMessage(ex);
                Debug.LogError($"Password reset email error: {errorMessage}");
                throw new Exception(errorMessage);
            }
        }
        else
        {
            Debug.LogError("Please enter your email");
            throw new Exception("Please enter your email");
        }
    }

    [TabGroup("Actions")]
    [Button("Sign In with Google", ButtonSizes.Large)]
    private async Task SignInWithGoogle()
    {
        try
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
        catch (FirebaseException ex)
        {
            string errorMessage = GetFirebaseErrorMessage(ex);
            Debug.LogError($"Google sign in error: {errorMessage}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Unknown error during Google sign in: {ex.Message}");
        }
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