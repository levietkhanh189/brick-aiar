using UnityEngine;
using Sirenix.OdinInspector;
using API;

/// <summary>
/// Helper script để setup scene với tất cả components cần thiết cho AI Flow
/// </summary>
public class SceneSetupHelper : MonoBehaviour
{
    [Title("AI Flow Scene Setup Helper")]
    [InfoBox("Script này giúp setup scene với tất cả components cần thiết cho AI Flow hoạt động.", InfoMessageType.Info)]
    
    [FoldoutGroup("Current Scene Status")]
    [ShowInInspector, ReadOnly]
    [LabelText("APIManager exists")]
    private bool hasAPIManager => FindObjectOfType<APIManager>() != null;
    
    [FoldoutGroup("Current Scene Status")]
    [ShowInInspector, ReadOnly]
    [LabelText("FirebaseAuthManager exists")]
    private bool hasFirebaseAuth => FindObjectOfType<FirebaseAuthManager>() != null;
    
    [FoldoutGroup("Current Scene Status")]
    [ShowInInspector, ReadOnly]
    [LabelText("AIFlowDemo exists")]
    private bool hasAIFlowDemo => FindObjectOfType<AIFlowDemo>() != null;
    
    [FoldoutGroup("Current Scene Status")]
    [ShowInInspector, ReadOnly]
    [LabelText("All Required Components")]
    private bool allRequiredExists => hasAPIManager && hasFirebaseAuth;
    
    [Button("🏗️ Setup Complete Scene", ButtonSizes.Large)]
    [InfoBox("Tạo tất cả GameObjects cần thiết với scripts đã gắn sẵn")]
    public void SetupCompleteScene()
    {
        SetupAPIManager();
        SetupFirebaseAuth();
        SetupAIFlowDemo();
        
        Debug.Log("✅ Scene setup hoàn tất! Tất cả components cần thiết đã được tạo.");
    }
    
    [Button("🔧 Setup APIManager", ButtonSizes.Medium)]
    [HideIf("hasAPIManager")]
    public void SetupAPIManager()
    {
        if (FindObjectOfType<APIManager>() != null)
        {
            Debug.Log("APIManager đã tồn tại!");
            return;
        }
        
        GameObject apiManagerGO = new GameObject("🔧 API_Manager");
        apiManagerGO.AddComponent<APIManager>();
        
        Debug.Log("✅ Đã tạo APIManager");
    }
    
    [Button("🔐 Setup Firebase Auth", ButtonSizes.Medium)]
    [HideIf("hasFirebaseAuth")]
    public void SetupFirebaseAuth()
    {
        if (FindObjectOfType<FirebaseAuthManager>() != null)
        {
            Debug.Log("FirebaseAuthManager đã tồn tại!");
            return;
        }
        
        GameObject firebaseGO = new GameObject("🔐 Firebase_Auth");
        firebaseGO.AddComponent<FirebaseAuthManager>();
        
        Debug.Log("✅ Đã tạo FirebaseAuthManager");
    }
    
    [Button("🎮 Setup AI Flow Demo", ButtonSizes.Medium)]
    [HideIf("hasAIFlowDemo")]
    public void SetupAIFlowDemo()
    {
        if (FindObjectOfType<AIFlowDemo>() != null)
        {
            Debug.Log("AIFlowDemo đã tồn tại!");
            return;
        }
        
        GameObject demoGO = new GameObject("🎮 AI_Flow_Demo");
        demoGO.AddComponent<AIFlowDemo>();
        
        Debug.Log("✅ Đã tạo AIFlowDemo");
    }
    
    [Button("🗑️ Clean Up Scene", ButtonSizes.Small)]
    [InfoBox("Xóa tất cả AI Flow components khỏi scene")]
    public void CleanUpScene()
    {
        // Tìm và xóa tất cả components
        var apiManager = FindObjectOfType<APIManager>();
        var firebaseAuth = FindObjectOfType<FirebaseAuthManager>();
        var aiDemo = FindObjectOfType<AIFlowDemo>();
        
        if (apiManager != null)
        {
            DestroyImmediate(apiManager.gameObject);
            Debug.Log("🗑️ Đã xóa APIManager");
        }
        
        if (firebaseAuth != null)
        {
            DestroyImmediate(firebaseAuth.gameObject);
            Debug.Log("🗑️ Đã xóa FirebaseAuthManager");
        }
        
        if (aiDemo != null)
        {
            DestroyImmediate(aiDemo.gameObject);
            Debug.Log("🗑️ Đã xóa AIFlowDemo");
        }
        
        Debug.Log("✅ Scene cleanup hoàn tất!");
    }
    
    [Title("Setup Instructions")]
    [InfoBox(@"**Thứ tự setup:**
1. Nhấn '🏗️ Setup Complete Scene' để tạo tất cả
2. Hoặc nhấn từng nút riêng lẻ
3. Đảm bảo Firebase được cấu hình đúng
4. User cần đăng nhập Firebase trước khi dùng

**Scripts bắt buộc:**
• APIManager - Quản lý API calls
• FirebaseAuthManager - Xác thực user

**Scripts tùy chọn:**
• AIFlowDemo - Demo với Odin Inspector", InfoMessageType.Info)]
    
    [Button("📖 Open Documentation")]
    public void OpenDocumentation()
    {
        string readmePath = "Assets/_Main/Scripts/API/README_AI_Flow.md";
        Application.OpenURL("file://" + Application.dataPath.Replace("Assets", "") + readmePath);
    }
    
    void Start()
    {
        // Kiểm tra setup khi scene load
        if (!allRequiredExists)
        {
            Debug.LogWarning("⚠️ Scene chưa được setup đầy đủ! Vui lòng chạy SceneSetupHelper.");
        }
        else
        {
            Debug.Log("✅ Scene đã được setup đầy đủ cho AI Flow!");
        }
    }
} 