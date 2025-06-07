using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace API
{
    /// <summary>
    /// Utility class để tải xuống file LDR từ API presigned URL
    /// </summary>
    public static class LDRDownloader
    {
        private const string API_URL = "https://lvm3bok3icqnfhj2o7llcfxbbe0vwbxv.lambda-url.us-east-1.on.aws/";

        [System.Serializable]
        public class GetPresignedUrlRequest
        {
            public string path = "/get";
            public string user_id;
            public OptionsData options;
            public string s3_path;

            [System.Serializable]
            public class OptionsData
            {
                public bool authRequired = true;
            }
        }

        [System.Serializable]
        public class GetPresignedUrlResponse
        {
            public string requestId;
            public string status;
            public string modelUrl;
        }

        /// <summary>
        /// Tải xuống file LDR từ S3 path thông qua API presigned URL
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <param name="s3Path">Đường dẫn S3 của file LDR</param>
        /// <param name="onComplete">Callback với file content (string) hoặc error</param>
        public static void DownloadLDRFileFromS3Path(string userId, string s3Path, Action<string, string> onComplete)
        {
            LDRDownloaderHelper.Instance.StartCoroutineHelper(DownloadLDRFileFromS3PathCoroutine(userId, s3Path, onComplete));
        }

        /// <summary>
        /// Coroutine để tải xuống file LDR từ S3 path thông qua API presigned URL
        /// </summary>
        private static IEnumerator DownloadLDRFileFromS3PathCoroutine(string userId, string s3Path, Action<string, string> onComplete)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(s3Path))
            {
                onComplete?.Invoke(null, "User ID hoặc S3 path không hợp lệ");
                yield break;
            }

            Debug.Log($"Bắt đầu lấy presigned URL cho file LDR: {s3Path}");

            // Bước 1: Gọi API để lấy presigned URL
            string presignedUrl = null;
            string error = null;
            bool completed = false;

            yield return GetPresignedUrl(userId, s3Path, (url, err) =>
            {
                presignedUrl = url;
                error = err;
                completed = true;
            });

            yield return new WaitUntil(() => completed);

            if (!string.IsNullOrEmpty(error))
            {
                onComplete?.Invoke(null, error);
                yield break;
            }

            // Bước 2: Tải xuống file từ presigned URL
            yield return DownloadLDRFile(presignedUrl, onComplete);
        }

        /// <summary>
        /// Gọi API để lấy presigned URL
        /// </summary>
        private static IEnumerator GetPresignedUrl(string userId, string s3Path, Action<string, string> onComplete)
        {
            var requestData = new GetPresignedUrlRequest
            {
                user_id = userId,
                s3_path = s3Path,
                options = new GetPresignedUrlRequest.OptionsData()
            };

            string jsonData = JsonUtility.ToJson(requestData);
            Debug.Log($"Gửi request API: {jsonData}");

            using (UnityWebRequest www = new UnityWebRequest(API_URL, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
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
                    string errorMsg = $"Lỗi gọi API presigned URL: {www.error} - Response Code: {www.responseCode}";
                    Debug.LogError(errorMsg);
                    onComplete?.Invoke(null, errorMsg);
                }
                else
                {
                    try
                    {
                        string responseText = www.downloadHandler.text;
                        Debug.Log($"API Response: {responseText}");

                        var response = JsonUtility.FromJson<GetPresignedUrlResponse>(responseText);
                        
                        if (!string.IsNullOrEmpty(response.modelUrl))
                        {
                            Debug.Log($"Lấy presigned URL thành công: {response.modelUrl}");
                            onComplete?.Invoke(response.modelUrl, null);
                        }
                        else
                        {
                            onComplete?.Invoke(null, $"Response không chứa modelUrl. Status: {response.status}");
                        }
                    }
                    catch (Exception e)
                    {
                        onComplete?.Invoke(null, $"Lỗi parse response: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Tải xuống file LDR từ presigned URL
        /// </summary>
        /// <param name="presignedUrl">Presigned URL của file LDR</param>
        /// <param name="onComplete">Callback với file content (string) hoặc error</param>
        /// <returns>Coroutine</returns>
        public static IEnumerator DownloadLDRFile(string presignedUrl, Action<string, string> onComplete)
        {
            if (string.IsNullOrEmpty(presignedUrl))
            {
                onComplete?.Invoke(null, "Presigned URL không hợp lệ");
                yield break;
            }

            Debug.Log($"Bắt đầu tải xuống file LDR từ presigned URL: {presignedUrl}");

            using (UnityWebRequest www = UnityWebRequest.Get(presignedUrl))
            {
                yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
#else
                if (www.isNetworkError || www.isHttpError)
#endif
                {
                    string errorMsg = $"Lỗi tải xuống file LDR: {www.error} - Response Code: {www.responseCode}";
                    Debug.LogError(errorMsg);
                    onComplete?.Invoke(null, errorMsg);
                }
                else
                {
                    string ldrContent = www.downloadHandler.text;
                    if (!string.IsNullOrEmpty(ldrContent))
                    {
                        Debug.Log($"Tải xuống file LDR thành công! Size: {ldrContent.Length} characters");
                        onComplete?.Invoke(ldrContent, null);
                    }
                    else
                    {
                        onComplete?.Invoke(null, "File LDR rỗng");
                    }
                }
            }
        }

        /// <summary>
        /// Lưu file LDR vào local storage
        /// </summary>
        /// <param name="ldrContent">Nội dung file LDR</param>
        /// <param name="fileName">Tên file (không cần extension .ldr)</param>
        /// <returns>Đường dẫn file đã lưu hoặc null nếu lỗi</returns>
        public static string SaveLDRToLocal(string ldrContent, string fileName)
        {
            try
            {
                string folderPath = Path.Combine(Application.persistentDataPath, "LDRFiles");
                
                // Tạo thư mục nếu chưa có
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string filePath = Path.Combine(folderPath, $"{fileName}.ldr");
                File.WriteAllText(filePath, ldrContent);
                
                Debug.Log($"Đã lưu file LDR: {filePath}");
                return filePath;
            }
            catch (Exception e)
            {
                Debug.LogError($"Lỗi lưu file LDR: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Lấy tên file từ S3 path
        /// </summary>
        /// <param name="s3Path">S3 path</param>
        /// <returns>Tên file không có extension</returns>
        public static string GetFileNameFromS3Path(string s3Path)
        {
            try
            {
                // s3://gen3d-output/GGkMA2w7gjT4d3VZWVmGzbsoVNq2/3844b650-d234-4907-a6e5-7ef8fd091e52/model.ldr
                string[] parts = s3Path.Split('/');
                string fileName = Path.GetFileNameWithoutExtension(parts[parts.Length - 1]);
                return fileName;
            }
            catch
            {
                return $"lego_model_{DateTime.Now:yyyyMMdd_HHmmss}";
            }
        }

        /// <summary>
        /// Kiểm tra xem file LDR đã tồn tại trong local storage chưa
        /// </summary>
        /// <param name="fileName">Tên file (không cần extension)</param>
        /// <returns>True nếu file đã tồn tại</returns>
        public static bool IsLDRFileExists(string fileName)
        {
            string folderPath = Path.Combine(Application.persistentDataPath, "LDRFiles");
            string filePath = Path.Combine(folderPath, $"{fileName}.ldr");
            return File.Exists(filePath);
        }

        /// <summary>
        /// Đọc file LDR từ local storage
        /// </summary>
        /// <param name="fileName">Tên file (không cần extension)</param>
        /// <returns>Nội dung file hoặc null nếu không tồn tại</returns>
        public static string ReadLocalLDRFile(string fileName)
        {
            try
            {
                string folderPath = Path.Combine(Application.persistentDataPath, "LDRFiles");
                string filePath = Path.Combine(folderPath, $"{fileName}.ldr");
                
                if (File.Exists(filePath))
                {
                    return File.ReadAllText(filePath);
                }
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"Lỗi đọc file LDR local: {e.Message}");
                return null;
            }
        }
    }

    /// <summary>
    /// Helper class để start coroutine từ static context
    /// </summary>
    public class LDRDownloaderHelper : MonoBehaviour
    {
        private static LDRDownloaderHelper _instance;
        
        public static LDRDownloaderHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("LDRDownloaderHelper");
                    _instance = go.AddComponent<LDRDownloaderHelper>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        public void StartCoroutineHelper(IEnumerator coroutine)
        {
            StartCoroutine(coroutine);
        }
    }
} 