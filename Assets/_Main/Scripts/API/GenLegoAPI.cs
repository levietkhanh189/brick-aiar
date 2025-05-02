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
        public string user_id = "Right_test";
        public Options options = new Options { authRequired = true };
        public string image; // base64 string

        [Serializable]
        public class Options
        {
            public bool authRequired;
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

    public class GenLegoAPI
    {
        private const string apiUrl = "https://your-api-endpoint.com/gen_lego"; // Replace with actual API URL

        public IEnumerator GenLegoCoroutine(string base64Image, Action<GenLegoResponseBody, string> callback)
        {
            var requestData = new GenLegoRequest
            {
                image = base64Image
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
                        var responseBody = JsonUtility.FromJson<GenLegoResponseBody>(www.downloadHandler.text);
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
