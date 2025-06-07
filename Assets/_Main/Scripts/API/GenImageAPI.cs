using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace API
{
    [Serializable]
    public class GenImageRequest
    {
        public string path = "/gen_image";
        public string user_id;
        public Options options = new Options { authRequired = true };
        public string prompt;

        [Serializable]
        public class Options
        {
            public bool authRequired;
        }
    }

    [Serializable]
    public class GenImageResponseBody
    {
        public string requestId;
        public string image; // base64 image data
    }

    [Serializable]
    public class GenImageResponse
    {
        public int statusCode;
        public string body;
    }

    public class GenImageAPI
    {
        private const string apiUrl = "https://lvm3bok3icqnfhj2o7llcfxbbe0vwbxv.lambda-url.us-east-1.on.aws/";

        public IEnumerator GenImageCoroutine(string prompt, Action<GenImageResponseBody, string> callback)
        {
            // Lấy user_id từ FirebaseAuthManager
            string userId = "Right_test"; // Default fallback
            if (FirebaseAuthManager.Instance != null && !string.IsNullOrEmpty(FirebaseAuthManager.Instance.UserId))
            {
                userId = FirebaseAuthManager.Instance.UserId;
            }

            var requestData = new GenImageRequest
            {
                user_id = userId,
                prompt = prompt
            };

            string jsonData = JsonUtility.ToJson(requestData);
            Debug.Log($"Gửi request tạo ảnh: {jsonData}");

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
                        var responseBody = JsonUtility.FromJson<GenImageResponseBody>(www.downloadHandler.text);
                        Debug.Log(responseBody.image);

                        if (responseBody != null && !string.IsNullOrEmpty(responseBody.image))
                        {
                            Debug.Log("Tạo ảnh thành công!");
                            callback(responseBody, null);
                        }
                        else
                        {
                            callback(null, "Response không hợp lệ hoặc thiếu dữ liệu ảnh");
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
