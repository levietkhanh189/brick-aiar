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
        public string user_id = "Right_test";
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
        public string image;
    }

    [Serializable]
    public class GenImageResponse
    {
        public int statusCode;
        public string body;
    }

    public class GenImageAPI
    {
        private const string apiUrl = "https://your-api-endpoint.com/gen_image"; // Replace with actual API URL

        public IEnumerator GenImageCoroutine(string prompt, Action<GenImageResponseBody, string> callback)
        {
            var requestData = new GenImageRequest
            {
                prompt = prompt
            };

            string jsonData = JsonUtility.ToJson(requestData);

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
                    callback(null, www.error);
                }
                else
                {
                    try
                    {
                        var responseBody = JsonUtility.FromJson<GenImageResponseBody>(www.downloadHandler.text);
                        callback(responseBody, null);
                    }
                    catch (Exception e)
                    {
                        callback(null, e.Message);
                    }
                }
            }
        }
    }
}
