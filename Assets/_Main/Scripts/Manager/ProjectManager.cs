using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

public class ProjectManager : MonoBehaviour
{
    public LDrawImporter lDrawImporter;
    public List<GameObject> legoItems = new List<GameObject>();
    public LegoPanel legoPanel;

    [Header("Debug Info")]
    [SerializeField] private List<string> currentLocalPaths = new List<string>();

    public void Start()
    {
        if(lDrawImporter != null)
            LoadCurrentLocalLegoPaths();
    }

    /// <summary>
    /// Lấy danh sách các đường dẫn LDR file local hiện tại
    /// </summary>
    private void LoadCurrentLocalLegoPaths()
    {
        currentLocalPaths.Clear();
        
        // Method 1: Sử dụng LDRFileManager (primary method)
        if (LDRFileManager.Instance != null)
        {
            Debug.Log("Sử dụng LDRFileManager để lấy local paths...");
            LoadPathsFromLDRFileManager();
        }
        else
        {
            Debug.LogWarning("LDRFileManager.Instance không tồn tại, sử dụng fallback method...");
            // Method 2: Fallback - scan trực tiếp thư mục cache
            LoadPathsFromCacheDirectory();
        }
        
        Debug.Log($"Đã tìm thấy {currentLocalPaths.Count} lego paths local");
        
        // Tự động load các lego items nếu có
        if (currentLocalPaths.Count > 0)
        {
            LoadLegoItems(currentLocalPaths);
        }
        else
        {
            Debug.LogWarning("Không tìm thấy LDR files local nào");
        }
    }

    /// <summary>
    /// Lấy danh sách paths từ LDRFileManager trực tiếp
    /// </summary>
    private void LoadPathsFromLDRFileManager()
    {
        currentLocalPaths.Clear();
        
        if (LDRFileManager.Instance != null)
        {
            // Lấy tất cả model IDs đã cache
            List<string> cachedModelIds = LDRFileManager.Instance.GetCachedModelIds();
            
            foreach (string modelId in cachedModelIds)
            {
                // Lấy đường dẫn file local của từng model
                string localPath = LDRFileManager.Instance.GetLocalFilePath(modelId);
                if (!string.IsNullOrEmpty(localPath))
                {
                    currentLocalPaths.Add(localPath);
                }
            }
            
            Debug.Log($"Đã tìm thấy {currentLocalPaths.Count} LDR files từ LDRFileManager");
        }
        else
        {
            Debug.LogWarning("LDRFileManager.Instance không tồn tại");
        }
    }

    /// <summary>
    /// Fallback method: Scan trực tiếp thư mục cache để tìm LDR files
    /// </summary>
    private void LoadPathsFromCacheDirectory()
    {
        currentLocalPaths.Clear();
        
        try
        {
            // Thử các thư mục cache có thể có
            string[] possibleCachePaths = {
                Path.Combine(Application.persistentDataPath, "LDRCache"),
                Path.Combine(Application.persistentDataPath, "LDRFiles"),
                Path.Combine(Application.persistentDataPath, "Cache")
            };
            
            foreach (string cacheDir in possibleCachePaths)
            {
                if (Directory.Exists(cacheDir))
                {
                    Debug.Log($"Scanning cache directory: {cacheDir}");
                    string[] ldrFiles = Directory.GetFiles(cacheDir, "*.ldr");
                    
                    foreach (string filePath in ldrFiles)
                    {
                        if (File.Exists(filePath))
                        {
                            currentLocalPaths.Add(filePath);
                            Debug.Log($"Found LDR file: {filePath}");
                        }
                    }
                }
            }
            
            Debug.Log($"Đã scan và tìm thấy {currentLocalPaths.Count} LDR files trong cache directories");
        }
        catch (Exception e)
        {
            Debug.LogError($"Lỗi khi scan cache directory: {e.Message}");
        }
    }

    /// <summary>
    /// Refresh lại danh sách local paths
    /// </summary>
    public void RefreshLocalPaths()
    {
        LoadCurrentLocalLegoPaths();
    }

    /// <summary>
    /// Lấy danh sách local paths hiện tại
    /// </summary>
    public List<string> GetCurrentLocalPaths()
    {
        return new List<string>(currentLocalPaths);
    }

    public void LoadLegoItems(List<string> legoPaths)
    {
        legoItems = new List<GameObject>();
        foreach (var legoPath in legoPaths)
        {
            lDrawImporter.ImportLDrawModel(legoPath);
            lDrawImporter.currentModel.transform.SetParent(transform);
            if (lDrawImporter.currentModel != null)
            {
                legoItems.Add(lDrawImporter.currentModel);
            }
        }
        legoPanel.ShowAllLego(legoItems);
    }

    public void GoToHome()
    {
        GameSceneManager.Instance.LoadScene("MainScene");
    }
}
