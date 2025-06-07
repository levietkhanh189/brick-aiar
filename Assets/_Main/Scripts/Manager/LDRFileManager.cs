using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

public class LDRFileManager : MonoBehaviour
{
    private static LDRFileManager instance;
    private string cacheDirectory;
    private Dictionary<string, string> ldrCache = new Dictionary<string, string>();

    public static LDRFileManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<LDRFileManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("LDRFileManager");
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
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        cacheDirectory = Path.Combine(Application.persistentDataPath, "LDRCache");
        if (!Directory.Exists(cacheDirectory))
        {
            Directory.CreateDirectory(cacheDirectory);
        }
    }

    public async Task<string> GetLDRFile(string ldrUrl)
    {
        if (ldrCache.ContainsKey(ldrUrl))
        {
            return ldrCache[ldrUrl];
        }

        string fileName = Path.GetFileName(new System.Uri(ldrUrl).LocalPath);
        string filePath = Path.Combine(cacheDirectory, fileName);

        if (File.Exists(filePath))
        {
            ldrCache[ldrUrl] = filePath;
            return filePath;
        }

        // Nếu file chưa tồn tại, tải về và lưu vào cache
        using (var client = new System.Net.Http.HttpClient())
        {
            try
            {
                var response = await client.GetAsync(ldrUrl);
                response.EnsureSuccessStatusCode();
                byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();
                File.WriteAllBytes(filePath, fileBytes);
                ldrCache[ldrUrl] = filePath;
                Debug.Log($"Đã tải và lưu file LDR vào: {filePath}");
                return filePath;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Lỗi khi tải file LDR từ {ldrUrl}: {e.Message}");
                return null;
            }
        }
    }

    public void ClearCache()
    {
        if (Directory.Exists(cacheDirectory))
        {
            Directory.Delete(cacheDirectory, true);
            Directory.CreateDirectory(cacheDirectory);
            ldrCache.Clear();
            Debug.Log("Đã xóa bộ nhớ cache LDR.");
        }
    }
} 