using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace API
{
    /// <summary>
    /// Utility class để tải xuống file LDR từ S3 URL
    /// </summary>
    public static class LDRDownloader
    {
        /// <summary>
        /// Tải xuống file LDR từ URL
        /// </summary>
        /// <param name="s3Url">URL của file LDR trên S3</param>
        /// <param name="onComplete">Callback với file content (string) hoặc error</param>
        /// <returns>Coroutine</returns>
        public static IEnumerator DownloadLDRFile(string s3Url, Action<string, string> onComplete)
        {
            if (string.IsNullOrEmpty(s3Url))
            {
                onComplete?.Invoke(null, "URL không hợp lệ");
                yield break;
            }

            Debug.Log($"Bắt đầu tải xuống file LDR: {s3Url}");

            using (UnityWebRequest www = UnityWebRequest.Get(s3Url))
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
        /// Lấy tên file từ S3 URL
        /// </summary>
        /// <param name="s3Url">S3 URL</param>
        /// <returns>Tên file không có extension</returns>
        public static string GetFileNameFromS3Url(string s3Url)
        {
            try
            {
                Uri uri = new Uri(s3Url);
                string fileName = Path.GetFileNameWithoutExtension(uri.Segments[uri.Segments.Length - 1]);
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
} 