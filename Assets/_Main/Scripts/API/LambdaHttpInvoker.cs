using System;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine.UI;
using Firebase.Auth;
using System.Collections;
using System.Runtime.CompilerServices;

public class LambdaHttpInvoker : MonoBehaviour
{
    [Header("API Configuration")]
    [SerializeField] private string lambdaUrl = "https://lvm3bok3icqnfhj2o7llcfxbbe0vwbxv.lambda-url.us-east-1.on.aws/";
    [SerializeField] private string firebaseProjectId = "brick-aiar";

    [Header("Firebase Auth")]
    [SerializeField] private string emailAddress = "test@gmail.com";
    [SerializeField] private string password = "123456";

    [Header("UI Elements (Optional)")]
    [SerializeField] private RawImage generatedImageDisplay;
    [SerializeField] private Text statusText;

    private FirebaseAuth firebaseAuth;
    private string userId;
    private string authToken;

    async void Start()
    {
        // Initialize Firebase Auth
        firebaseAuth = FirebaseAuth.DefaultInstance;

        // Login to Firebase
        await LoginToFirebase();
    }

    public async Task LoginToFirebase()
    {
        try
        {
            // Sign in to Firebase
            AuthResult authResult = await firebaseAuth.SignInWithEmailAndPasswordAsync(emailAddress, password);
            FirebaseUser user = authResult.User;
            userId = user.UserId;

            Debug.Log($"Firebase login successful: {user.Email} (ID: {userId})");

            // Get ID token from Firebase
            authToken = await user.TokenAsync(true);
            Debug.Log("Firebase ID Token obtained");
        }
        catch (Exception e)
        {
            Debug.LogError($"Authentication error: {e.Message}");
            if (statusText != null) statusText.text = "Auth Error: " + e.Message;
        }
    }
    
    private async Task<bool> RefreshTokenIfNeeded()
    {
        try
        {
            // Get new token from Firebase
            FirebaseUser user = firebaseAuth.CurrentUser;
            if (user == null)
            {
                Debug.LogError("User not logged in. Cannot refresh token.");
                return false;
            }
            
            // Force refresh token
            authToken = await user.TokenAsync(true);
            Debug.Log("Firebase token refreshed");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to refresh token: {e.Message}");
            return false;
        }
    }

    private async Task<string> InvokeApiWithPayload(string jsonPayload)
    {
        // Make sure we have a valid token
        if (string.IsNullOrEmpty(authToken))
        {
            bool refreshed = await RefreshTokenIfNeeded();
            if (!refreshed)
            {
                throw new Exception("No valid authentication token available");
            }
        }

        try
        {
            Debug.Log($"Sending payload: {jsonPayload}");

            // Create web request
            using (UnityWebRequest request = new UnityWebRequest(lambdaUrl, "POST"))
            {
                // Add payload to request
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                
                // Set headers
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", "Bearer " + authToken);

                // Send request
                await request.SendWebRequest();

                // Check for network errors
                if (request.result == UnityWebRequest.Result.ConnectionError || 
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    // Check if it's an auth error (401 Unauthorized)
                    if (request.responseCode == 401)
                    {
                        Debug.LogWarning("Authentication token expired. Attempting to refresh...");
                        
                        if (await RefreshTokenIfNeeded())
                        {
                            // Retry with new token
                            return await InvokeApiWithPayload(jsonPayload);
                        }
                        else
                        {
                            throw new Exception("Failed to refresh token and retry API call");
                        }
                    }
                    else
                    {
                        throw new Exception($"API error: {request.error} (Code: {request.responseCode})");
                    }
                }

                // Return response
                return request.downloadHandler.text;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error invoking API: {e.Message}");
            throw;
        }
    }

    public async Task<string> GetSample(string sampleType = "gun")
    {
        try
        {
            // Create serializable request object
            SampleRequest request = new SampleRequest
            {
                path = "/sample",
                user_id = userId,
                options = new AuthOptions { authRequired = true },
                sample = sampleType
            };

            string result = await InvokeApiWithPayload(JsonUtility.ToJson(request));
            Debug.Log($"Sample response: {result}");

            // Parse the response
            LambdaResponse lambdaResponse = JsonUtility.FromJson<LambdaResponse>(result);
            SampleResponse response = JsonUtility.FromJson<SampleResponse>(lambdaResponse.body);

            if (statusText != null)
                statusText.text = $"Sample request ID: {response.requestId}, Status: {response.status}";

            return response.modelUrl;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error getting sample: {e.Message}");
            if (statusText != null) statusText.text = "Sample Error: " + e.Message;
            return null;
        }
    }

    public async Task<string> GenerateImage(string prompt)
    {
        try
        {
            // Create serializable request object
            ImageRequest request = new ImageRequest
            {
                path = "/gen_image",
                user_id = userId,
                options = new AuthOptions { authRequired = true },
                prompt = prompt
            };

            string result = await InvokeApiWithPayload(JsonUtility.ToJson(request));
            Debug.Log("Image generation response received");
            Debug.Log("Raw API result: " + result);

            LambdaResponse lambdaResponse = JsonUtility.FromJson<LambdaResponse>(result);
            if (lambdaResponse == null)
            {
                Debug.LogError("lambdaResponse is null. Raw result: " + result);
                if (statusText != null) statusText.text = "lambdaResponse is null";
                return null;
            }
            if (string.IsNullOrEmpty(lambdaResponse.body))
            {
                Debug.LogError("lambdaResponse.body is null or empty. Raw result: " + result);
                if (statusText != null) statusText.text = "lambdaResponse.body is null or empty";
                return null;
            }

            ImageResponse response = JsonUtility.FromJson<ImageResponse>(lambdaResponse.body);
            if (response == null)
            {
                Debug.LogError("ImageResponse is null. Body: " + lambdaResponse.body);
                if (statusText != null) statusText.text = "ImageResponse is null";
                return null;
            }

            if (statusText != null)
                statusText.text = $"Generated image - Request ID: {response.requestId}";

            // Display the image if RawImage is set
            if (!string.IsNullOrEmpty(response.image) && generatedImageDisplay != null)
            {
                byte[] imageBytes = Convert.FromBase64String(response.image);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(imageBytes);
                generatedImageDisplay.texture = texture;
            }

            return response.image; // Returns base64 image data
        }
        catch (Exception e)
        {
            Debug.LogError($"Error generating image: {e.Message}");
            if (statusText != null) statusText.text = "Generation Error: " + e.Message;
            return null;
        }
    }

    [Sirenix.OdinInspector.Button]
    // Example usage methods - can be called from UI buttons
    public async void OnSampleButtonClick()
    {
        await GetSample("gun");
    }

    public async void OnGenerateImageButtonClick(string prompt = "mini car 4 seat in cartoon, no background")
    {
        await GenerateImage(prompt);
    }

    public async Task<string> GenerateLego(string base64Image, float details = 0.02f, float foregroundRatio = 0.85f)
    {
        try
        {
            // Create serializable request object
            LegoRequest request = new LegoRequest
            {
                path = "/gen_lego",
                user_id = userId,
                options = new LegoOptions { 
                    authRequired = true,
                    details = details,
                    foregroundRatio = foregroundRatio
                },
                image = base64Image
            };

            string result = await InvokeApiWithPayload(JsonUtility.ToJson(request));
            Debug.Log("Lego generation response received");
            Debug.Log(result);
            
            // Parse the response
            LambdaResponse lambdaResponse = JsonUtility.FromJson<LambdaResponse>(result);
            LegoResponse response = JsonUtility.FromJson<LegoResponse>(lambdaResponse.body);

            if (statusText != null)
                statusText.text = $"Generated Lego - Request ID: {response.requestId}, Status: {response.status}";

            return response.modelUrl;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error generating Lego: {e.Message}");
            if (statusText != null) statusText.text = "Lego Generation Error: " + e.Message;
            return null;
        }
    }

    // Example usage method - can be called from UI buttons
    public async void OnGenerateLegoButtonClick(string base64Image)
    {
        await GenerateLego(base64Image);
    }

    // Example usage with custom parameters
    public async void OnGenerateLegoWithParamsButtonClick(string base64Image)
    {
        // Default detail level is 0.02 (fewer blocks), 0.01 for higher detail (more blocks)
        // Default background removal ratio is 0.85
        await GenerateLego(base64Image, 0.02f, 0.85f);
    }

    // Method to create Lego model from text
    public async Task<string> GenerateLegoFromText(string prompt, float details = 0.02f, float foregroundRatio = 0.85f)
    {
        try
        {
            // Step 1: Generate image from text
            string base64Image = await GenerateImage(prompt);
            
            if (string.IsNullOrEmpty(base64Image))
            {
                Debug.LogError("Failed to generate image from text");
                if (statusText != null) statusText.text = "Failed to generate image from text";
                return null;
            }
            
            // Step 2: Convert image to Lego model
            return await GenerateLego(base64Image, details, foregroundRatio);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error generating Lego from text: {e.Message}");
            if (statusText != null) statusText.text = "Text-to-Lego Error: " + e.Message;
            return null;
        }
    }
    
    // Utility method for UI calls
    public async void OnGenerateLegoFromTextButtonClick(string prompt)
    {
        string modelUrl = await GenerateLegoFromText(prompt);
        Debug.Log("modelUrl : " + modelUrl); 
    }
    
    // Utility method with custom parameters
    public async void OnGenerateLegoFromTextWithParamsButtonClick(string prompt, float details, float foregroundRatio)
    {
        await GenerateLegoFromText(prompt, details, foregroundRatio);
    }

    // Serializable request/response classes for JsonUtility
    [Serializable]
    public class AuthOptions
    {
        public bool authRequired;
    }

    [Serializable]
    public class SampleRequest
    {
        public string path;
        public string user_id;
        public AuthOptions options;
        public string sample;
    }

    [Serializable]
    public class ImageRequest
    {
        public string path;
        public string user_id;
        public AuthOptions options;
        public string prompt;
    }

    [Serializable]
    public class LambdaResponse
    {
        public int statusCode;
        public string body;
    }

    [Serializable]
    public class SampleResponse
    {
        public string requestId;
        public string status;
        public string modelUrl;
    }

    [Serializable]
    public class ImageResponse
    {
        public string requestId;
        public string status;
        public string image;
    }

    [Serializable]
    public class LegoOptions
    {
        public bool authRequired;
        public float details;
        public float foregroundRatio;
    }

    [Serializable]
    public class LegoRequest
    {
        public string path;
        public string user_id;
        public LegoOptions options;
        public string image;
    }

    [Serializable]
    public class LegoResponse
    {
        public string requestId;
        public string status;
        public string modelUrl;
    }
}

// Extension method to make UnityWebRequest awaitable
public static class UnityWebRequestExtension
{
    public static TaskAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOp)
    {
        var tcs = new TaskCompletionSource<object>();
        asyncOp.completed += obj => { tcs.SetResult(null); };
        return ((Task)tcs.Task).GetAwaiter();
    }
}