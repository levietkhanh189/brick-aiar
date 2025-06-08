using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace API
{
    public static class LDRDownloader
    {
        private const string API_URL = "https://lvm3bok3icqnfhj2o7llcfxbbe0vwbxv.lambda-url.us-east-1.on.aws/";

        [System.Serializable]
        public class APIRequest
        {
            public string path = "/get";
            public string user_id;
            public OptionsData options = new OptionsData();
            public string s3_path;

            [System.Serializable]
            public class OptionsData
            {
                public bool authRequired = true;
            }
        }

        [System.Serializable]
        public class APIResponse
        {
            public string modelUrl;
            public string status;
        }

        [System.Serializable]
        private class APIResult
        {
            public string url;
            public string error;
        }

        /// <summary>
        /// Tải file LDR từ S3 path
        /// </summary>
        public static void DownloadLDR(string userId, string s3Path, Action<string, string> onComplete)
        {
            CoroutineRunner.Instance.StartCoroutine(DownloadProcess(userId, s3Path, onComplete));
        }

        /// <summary>
        /// Wrapper method để tương thích với AIFlowDemo
        /// </summary>
        public static IEnumerator DownloadLDRFile(string s3Url, Action<string, string> onComplete)
        {
            string userId = FirebaseAuthManager.Instance.GetCurrentUserId();
            yield return DownloadProcess(userId, s3Url, onComplete);
        }

        /// <summary>
        /// Wrapper method để tương thích với AIFlowDemo
        /// </summary>
        public static void DownloadLDRFileFromS3Path(string userId, string s3Path, Action<string, string> onComplete)
        {
            CoroutineRunner.Instance.StartCoroutine(DownloadProcess(userId, s3Path, onComplete));
        }

        private static IEnumerator DownloadProcess(string userId, string s3Path, Action<string, string> onComplete)
        {
            // Bước 1: Lấy presigned URL
            var apiResult = new APIResult();
            yield return GetPresignedURL(userId, s3Path, apiResult);

            if (!string.IsNullOrEmpty(apiResult.error))
            {
                onComplete?.Invoke(null, apiResult.error);
                yield break;
            }

            // Bước 2: Tải file từ presigned URL
            yield return DownloadFile(apiResult.url, onComplete);
        }

        private static IEnumerator GetPresignedURL(string userId, string s3Path, APIResult result)
        {
            var request = new APIRequest
            {
                user_id = userId,
                s3_path = s3Path
            };

            string json = JsonUtility.ToJson(request);

            using (var www = new UnityWebRequest(API_URL, "POST"))
            {
                www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");

                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    result.error = $"API Error: {www.error}";
                }
                else
                {
                    try
                    {
                        var response = JsonUtility.FromJson<APIResponse>(www.downloadHandler.text);
                        result.url = response.modelUrl;

                        if (string.IsNullOrEmpty(result.url))
                        {
                            result.error = "No URL in response";
                        }
                    }
                    catch (Exception e)
                    {
                        result.error = $"Parse error: {e.Message}";
                    }
                }
            }
        }

        private static IEnumerator DownloadFile(string url, Action<string, string> onComplete)
        {
            using (var www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    onComplete?.Invoke(null, $"Download error: {www.error}");
                }
                else
                {
                    string content = www.downloadHandler.text;
                    onComplete?.Invoke(content, null);
                }
            }
        }

        /// <summary>
        /// Lưu nội dung LDR vào file local
        /// </summary>
        public static string SaveToLocal(string content, string fileName)
        {
            try
            {
                string folder = Path.Combine(Application.persistentDataPath, "LDRFiles");
                Directory.CreateDirectory(folder);

                string filePath = Path.Combine(folder, $"{fileName}.ldr");
                File.WriteAllText(filePath, content);

                return filePath;
            }
            catch (Exception e)
            {
                Debug.LogError($"Save error: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Lưu nội dung LDR vào file local - wrapper để tương thích với AIFlowDemo
        /// </summary>
        public static string SaveLDRToLocal(string content, string fileName)
        {
            return SaveToLocal(content, fileName);
        }

        /// <summary>
        /// Đọc file LDR từ local
        /// </summary>
        public static string ReadFromLocal(string fileName)
        {
            try
            {
                string filePath = Path.Combine(Application.persistentDataPath, "LDRFiles", $"{fileName}.ldr");
                return File.Exists(filePath) ? File.ReadAllText(filePath) : null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Kiểm tra file có tồn tại không
        /// </summary>
        public static bool FileExists(string fileName)
        {
            string filePath = Path.Combine(Application.persistentDataPath, "LDRFiles", $"{fileName}.ldr");
            return File.Exists(filePath);
        }

        /// <summary>
        /// Lấy tên file từ S3 path
        /// </summary>
        public static string GetFileName(string s3Path)
        {
            try
            {
                return Path.GetFileNameWithoutExtension(s3Path.Split('/')[^1]);
            }
            catch
            {
                return $"model_{DateTime.Now:yyyyMMddHHmmss}";
            }
        }

        /// <summary>
        /// Lấy tên file từ S3 path - wrapper để tương thích với AIFlowDemo
        /// </summary>
        public static string GetFileNameFromS3Path(string s3Path)
        {
            return GetFileName(s3Path);
        }
    }

    /// <summary>
    /// Helper để chạy coroutine từ static context
    /// </summary>
    public class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner instance;
        
        public static CoroutineRunner Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("CoroutineRunner");
                    instance = go.AddComponent<CoroutineRunner>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }
    }
}