using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace API
{
    [Serializable]
    public class GenLegoRequest
    {
        public string path = "/gen_lego";
        public string user_id;
        public Options options = new Options { authRequired = true };
        public string image; // base64 string

        [Serializable]
        public class Options
        {
            public bool authRequired;
            public float details = 0.02f;
            public float foregroundRatio = 0.85f;
        }
    }

    [Serializable]
    public class GenLegoResponseBody
    {
        public string requestId;
        public string status;
        public string modelUrl;
    }

    [Serializable]
    public class GenLegoResponse
    {
        public int statusCode;
        public string body;
    }

    /// <summary>
    /// Data structure cho LEGO model từ Firebase
    /// </summary>
    [Serializable]
    public class LegoModelData
    {
        public string requestId;
        public string user_id;
        public string category;
        public float created_at;
        public string description;
        public string model_url; // S3 path to .ldr file
        public string name;
        public string polycount;
        public string price;
        public string status; // "processing", "success", "failed"
        public string thumbnail_url;
    }

    public class GenLegoAPI
    {
        private const string apiUrl = "https://lvm3bok3icqnfhj2o7llcfxbbe0vwbxv.lambda-url.us-east-1.on.aws/";

        public IEnumerator GenLegoCoroutine(string base64Image, Action<GenLegoResponseBody, string> callback, 
            float details = 0.02f, float foregroundRatio = 0.85f)
        {
            // Lấy user_id từ FirebaseAuthManager
            string userId = "Right_test"; // Default fallback
            if (FirebaseAuthManager.Instance != null && !string.IsNullOrEmpty(FirebaseAuthManager.Instance.UserId))
            {
                userId = FirebaseAuthManager.Instance.UserId;
            }

            var requestData = new GenLegoRequest
            {
                user_id = userId,
                image = base64Image,
                options = new GenLegoRequest.Options
                {
                    authRequired = true,
                    details = details,
                    foregroundRatio = foregroundRatio
                }
            };

            string jsonData = JsonUtility.ToJson(requestData);
            Debug.Log($"Gửi request tạo LEGO: User ID = {userId}, Details = {details}, ForegroundRatio = {foregroundRatio}");

            using (UnityWebRequest www = new UnityWebRequest(apiUrl, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");

                yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
#else
                if (www.isNetworkError || www.isHttpError)
#endif
                {
                    string errorMsg = $"Lỗi kết nối: {www.error} - Response Code: {www.responseCode}";
                    Debug.LogError(errorMsg);
                    callback(null, errorMsg);
                }
                else
                {
                    try
                    {
                        Debug.Log($"Response nhận được: {www.downloadHandler.text}");
                        var responseBody = JsonUtility.FromJson<GenLegoResponseBody>(www.downloadHandler.text);
                        if (responseBody != null && !string.IsNullOrEmpty(responseBody.requestId))
                        {
                            Debug.Log($"Gửi request LEGO thành công! Request ID: {responseBody.requestId}, Status: {responseBody.status}");
                            callback(responseBody, null);
                        }
                        else
                        {
                            callback(null, "Response không hợp lệ hoặc thiếu requestId");
                        }
                    }
                    catch (Exception e)
                    {
                        string errorMsg = $"Lỗi parse JSON: {e.Message}";
                        Debug.LogError(errorMsg);
                        callback(null, errorMsg);
                    }
                }
            }
        }
    }
}
