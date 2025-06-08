using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using API;
using UnityEngine.Networking;
using System.Collections;

[System.Serializable]
public class LDRCacheData
{
    public string modelId;
    public string s3LdrUrl;
    public string localFilePath;
    public long fileSize;
    public string lastModified;

    public LDRCacheData(string modelId, string s3LdrUrl, string localFilePath)
    {
        this.modelId = modelId;
        this.s3LdrUrl = s3LdrUrl;
        this.localFilePath = localFilePath;

        if (File.Exists(localFilePath))
        {
            FileInfo fileInfo = new FileInfo(localFilePath);
            this.fileSize = fileInfo.Length;
            this.lastModified = fileInfo.LastWriteTime.ToString();
        }
    }
}

[System.Serializable]
public class CacheDataList
{
    public List<LDRCacheData> cacheList = new List<LDRCacheData>();
}

public class LDRFileManager : MonoBehaviour
{
    private static LDRFileManager instance;
    private string cacheDirectory;
    private string cacheIndexFile;

    // Dictionary để map model_id với cache data
    private Dictionary<string, LDRCacheData> modelIdCache = new Dictionary<string, LDRCacheData>();
    // Dictionary để map URL với local path (giữ lại để tương thích)
    private Dictionary<string, string> urlCache = new Dictionary<string, string>();

    // Events
    public static event Action<string, string> OnLDRFileDownloaded; // modelId, filePath
    public static event Action<string, string> OnLDRFileError; // modelId, error

    public static LDRFileManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<LDRFileManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("LDR File Manager");
                    instance = obj.AddComponent<LDRFileManager>();
                }
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeCache();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void InitializeCache()
    {
        cacheDirectory = Path.Combine(Application.persistentDataPath, "LDRCache");
        cacheIndexFile = Path.Combine(cacheDirectory, "cache_index.json");

        if (!Directory.Exists(cacheDirectory))
        {
            Directory.CreateDirectory(cacheDirectory);
        }

        LoadCacheIndex();
    }

    private void LoadCacheIndex()
    {
        if (File.Exists(cacheIndexFile))
        {
            try
            {
                string jsonContent = File.ReadAllText(cacheIndexFile);
                var wrapper = JsonUtility.FromJson<CacheDataList>(jsonContent);

                if (wrapper?.cacheList != null)
                {
                    foreach (var cacheData in wrapper.cacheList)
                    {
                        // Kiểm tra file có tồn tại không
                        if (File.Exists(cacheData.localFilePath))
                        {
                            modelIdCache[cacheData.modelId] = cacheData;
                            urlCache[cacheData.s3LdrUrl] = cacheData.localFilePath;
                        }
                    }
                }

                Debug.Log($"Đã tải {modelIdCache.Count} mục từ cache index.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Lỗi khi tải cache index: {e.Message}");
            }
        }
    }

    private void SaveCacheIndex()
    {
        try
        {
            var wrapper = new CacheDataList();
            wrapper.cacheList = new List<LDRCacheData>(modelIdCache.Values);
            string jsonContent = JsonUtility.ToJson(wrapper, true);
            File.WriteAllText(cacheIndexFile, jsonContent);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Lỗi khi lưu cache index: {e.Message}");
        }
    }

    // Phương thức chính: Lấy file theo model_id
    public void GetLDRFileByModelId(string modelId, string userId, string s3LdrUrl, Action<string, string> onComplete = null)
    {
        // Kiểm tra cache theo model_id trước
        if (modelIdCache.ContainsKey(modelId))
        {
            string cachedPath = modelIdCache[modelId].localFilePath;
            if (File.Exists(cachedPath))
            {
                Debug.Log($"Tìm thấy file LDR cho model {modelId} trong cache: {cachedPath}");
                onComplete?.Invoke(cachedPath, null);
                OnLDRFileDownloaded?.Invoke(modelId, cachedPath);
                return;
            }
            else
            {
                // File không tồn tại, xóa khỏi cache
                modelIdCache.Remove(modelId);
                if (urlCache.ContainsKey(s3LdrUrl))
                {
                    urlCache.Remove(s3LdrUrl);
                }
            }
        }

        // Nếu không có trong cache hoặc file không tồn tại, tải về
        DownloadAndCacheLDRFile(modelId, userId, s3LdrUrl, onComplete);
    }

    private void DownloadAndCacheLDRFile(string modelId, string userId, string s3LdrUrl, Action<string, string> onComplete)
    {
        Debug.Log($"[LDRFileManager] Bắt đầu tải LDR file cho model: {modelId}");
        Debug.Log($"[LDRFileManager] UserId: {userId}");
        Debug.Log($"[LDRFileManager] S3LdrUrl: {s3LdrUrl}");

        // Kiểm tra xem s3LdrUrl đã là presigned URL chưa hay vẫn là S3 path
        bool isPresignedUrl = s3LdrUrl.StartsWith("http://") || s3LdrUrl.StartsWith("https://");
        
        if (isPresignedUrl)
        {
            Debug.Log($"[LDRFileManager] URL đã là presigned URL, tải trực tiếp: {s3LdrUrl}");
            // Nếu đã là presigned URL, tải trực tiếp
            StartCoroutine(DownloadFromPresignedUrl(modelId, s3LdrUrl, onComplete));
        }
        else
        {
            Debug.Log($"[LDRFileManager] URL là S3 path, cần lấy presigned URL: {s3LdrUrl}");
            // Nếu là S3 path, gọi API để lấy presigned URL
            LDRDownloader.DownloadLDR(userId, s3LdrUrl, (ldrContent, error) =>
            {
                HandleDownloadResult(modelId, s3LdrUrl, ldrContent, error, onComplete);
            });
        }
    }

    private IEnumerator DownloadFromPresignedUrl(string modelId, string presignedUrl, Action<string, string> onComplete)
    {
        Debug.Log($"[LDRFileManager] Tải từ presigned URL: {presignedUrl}");
        
        using (var www = UnityWebRequest.Get(presignedUrl))
        {
            www.timeout = 30;
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                string error = $"Direct download error: {www.error}";
                Debug.LogError($"[LDRFileManager] {error}");
                HandleDownloadResult(modelId, presignedUrl, null, error, onComplete);
            }
            else
            {
                string content = www.downloadHandler.text;
                Debug.Log($"[LDRFileManager] Tải trực tiếp thành công, kích thước: {content?.Length ?? 0}");
                HandleDownloadResult(modelId, presignedUrl, content, null, onComplete);
            }
        }
    }

    private void HandleDownloadResult(string modelId, string s3LdrUrl, string ldrContent, string error, Action<string, string> onComplete)
    {
        if (!string.IsNullOrEmpty(error))
        {
            Debug.LogError($"[LDRFileManager] Lỗi tải LDR file cho model {modelId}: {error}");
            onComplete?.Invoke(null, error);
            OnLDRFileError?.Invoke(modelId, error);
            return;
        }

        if (string.IsNullOrEmpty(ldrContent))
        {
            string errorMsg = "LDR content rỗng";
            Debug.LogError($"[LDRFileManager] Model {modelId}: {errorMsg}");
            onComplete?.Invoke(null, errorMsg);
            OnLDRFileError?.Invoke(modelId, errorMsg);
            return;
        }

        // Lưu file vào cache
        string fileName = $"{modelId}_{DateTime.Now:yyyyMMddHHmmss}";
        string filePath = Path.Combine(cacheDirectory, $"{fileName}.ldr");

        try
        {
            File.WriteAllText(filePath, ldrContent);

            // Lưu vào cache dictionary
            var cacheData = new LDRCacheData(modelId, s3LdrUrl, filePath);
            modelIdCache[modelId] = cacheData;
            urlCache[s3LdrUrl] = filePath;

            // Lưu cache index
            SaveCacheIndex();

            Debug.Log($"[LDRFileManager] Đã tải và lưu LDR file cho model {modelId}: {filePath}");
            onComplete?.Invoke(filePath, null);
            OnLDRFileDownloaded?.Invoke(modelId, filePath);
        }
        catch (Exception e)
        {
            string errorMsg = $"Lỗi lưu file: {e.Message}";
            Debug.LogError($"[LDRFileManager] Model {modelId}: {errorMsg}");
            onComplete?.Invoke(null, errorMsg);
            OnLDRFileError?.Invoke(modelId, errorMsg);
        }
    }

    // Kiểm tra xem model đã có trong cache chưa
    public bool IsModelCached(string modelId)
    {
        return modelIdCache.ContainsKey(modelId) &&
               File.Exists(modelIdCache[modelId].localFilePath);
    }

    // Lấy thông tin cache của model
    public LDRCacheData GetModelCacheInfo(string modelId)
    {
        return modelIdCache.ContainsKey(modelId) ? modelIdCache[modelId] : null;
    }

    // Lấy đường dẫn file local của model
    public string GetLocalFilePath(string modelId)
    {
        if (modelIdCache.ContainsKey(modelId))
        {
            string path = modelIdCache[modelId].localFilePath;
            return File.Exists(path) ? path : null;
        }
        return null;
    }

    // Xóa cache của một model cụ thể
    public void RemoveModelFromCache(string modelId)
    {
        if (modelIdCache.ContainsKey(modelId))
        {
            var cacheData = modelIdCache[modelId];

            // Xóa file
            if (File.Exists(cacheData.localFilePath))
            {
                File.Delete(cacheData.localFilePath);
            }

            // Xóa khỏi cache
            modelIdCache.Remove(modelId);
            if (urlCache.ContainsKey(cacheData.s3LdrUrl))
            {
                urlCache.Remove(cacheData.s3LdrUrl);
            }

            // Cập nhật cache index
            SaveCacheIndex();

            Debug.Log($"Đã xóa model {modelId} khỏi cache.");
        }
    }

    // Lấy danh sách tất cả model đã cache
    public List<string> GetCachedModelIds()
    {
        return new List<string>(modelIdCache.Keys);
    }

    // Lấy tổng dung lượng cache
    public long GetCacheSize()
    {
        long totalSize = 0;
        foreach (var cacheData in modelIdCache.Values)
        {
            if (File.Exists(cacheData.localFilePath))
            {
                totalSize += cacheData.fileSize;
            }
        }
        return totalSize;
    }

    // Xóa toàn bộ cache
    public void ClearCache()
    {
        if (Directory.Exists(cacheDirectory))
        {
            Directory.Delete(cacheDirectory, true);
            Directory.CreateDirectory(cacheDirectory);

            modelIdCache.Clear();
            urlCache.Clear();

            Debug.Log("Đã xóa toàn bộ cache LDR.");
        }
    }

    // Dọn dẹp cache (xóa các file không còn trong index)
    public void CleanupCache()
    {
        if (!Directory.Exists(cacheDirectory)) return;

        var existingFiles = Directory.GetFiles(cacheDirectory, "*.ldr");
        var validFiles = new HashSet<string>();

        foreach (var cacheData in modelIdCache.Values)
        {
            validFiles.Add(cacheData.localFilePath);
        }

        int deletedCount = 0;
        foreach (var file in existingFiles)
        {
            if (!validFiles.Contains(file))
            {
                File.Delete(file);
                deletedCount++;
            }
        }

        Debug.Log($"Đã dọn dẹp {deletedCount} file không sử dụng từ cache.");
    }
}