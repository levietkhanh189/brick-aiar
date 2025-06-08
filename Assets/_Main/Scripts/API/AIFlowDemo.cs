using System.Collections;
using UnityEngine;
using API;
using System;
using Sirenix.OdinInspector;
using System.Collections.Generic;

/// <summary>
/// Demo script để minh họa flow AI hoàn chỉnh: Unity -> AWS -> Firebase
/// Sử dụng Odin Inspector cho demo nhanh chóng
/// </summary>
public class AIFlowDemo : MonoBehaviour
{
    [TabGroup("Input")]
    [TextArea(2, 4)]
    [LabelText("Prompt để tạo ảnh")]
    public string imagePrompt = "a red sports car on a mountain road";

    [TabGroup("Input")]
    [LabelText("Texture2D để tạo Lego")]
    public Texture2D image;

    [TabGroup("Input")]
    [TextArea(2, 4)]
    [LabelText("Base64 để tạo Lego")]
    public string imageBase = "a red sports car on a mountain road";

    [TabGroup("Input")]
    [TextArea(1, 3)]
    [LabelText("S3 URL để tải LDR")]
    [InfoBox("Nhập đường link S3 của file LDR để tải xuống trực tiếp")]
    public string s3LdrUrl = "";

    [TabGroup("Settings")]
    [Range(0.01f, 0.1f)]
    [LabelText("Chi tiết LEGO")]
    [InfoBox("Giá trị thấp hơn = chi tiết cao hơn")]
    public float legoDetails = 0.02f;
    
    [TabGroup("Settings")]
    [Range(0.1f, 1.0f)]
    [LabelText("Tỷ lệ foreground")]
    [InfoBox("Tỷ lệ loại bỏ background")]
    public float foregroundRatio = 0.85f;

    [TabGroup("Status")]
    [ShowInInspector, ReadOnly]
    [LabelText("Trạng thái hiện tại")]
    private string currentStatus = "Sẵn sàng";

    [TabGroup("Status")]
    [ShowInInspector, ReadOnly]
    [LabelText("Có ảnh để tạo LEGO")]
    private bool hasImageForLego => !string.IsNullOrEmpty(currentImageBase64);

    [TabGroup("Results")]
    [ShowInInspector, ReadOnly]
    [PreviewField(120, ObjectFieldAlignment.Left)]
    [LabelText("Ảnh đã tạo")]
    private Texture2D generatedImage;

    [TabGroup("Results")]
    [ShowInInspector, ReadOnly]
    [LabelText("Request ID hiện tại")]
    private string currentRequestId;

    [TabGroup("Results")]
    [ShowInInspector, ReadOnly]
    [LabelText("LEGO Model Data")]
    private API.LegoModelData currentLegoData;

    [TabGroup("Logs")]
    [ShowInInspector, ReadOnly]
    [TextArea(10, 15)]
    [LabelText("Log Messages")]
    private string logMessages = "";

    private string currentImageBase64;
    private List<string> logs = new List<string>();

    void Start()
    {
        UpdateStatus("Sẵn sàng. Nhấn 'Tạo Ảnh' để bắt đầu demo.");
    }

    [TabGroup("Actions")]
    [Button("🎨 Tạo Ảnh từ Prompt", ButtonSizes.Large)]
    [EnableIf("@!string.IsNullOrEmpty(imagePrompt)")]
    public void GenerateImage()
    {
        if (string.IsNullOrEmpty(imagePrompt))
        {
            UpdateStatus("Vui lòng nhập prompt!");
            return;
        }

        StartCoroutine(GenerateImageFlow());
    }

    [TabGroup("Actions")]
    [Button("🧱 Tạo LEGO từ Ảnh", ButtonSizes.Large)]
    [EnableIf("hasImageForLego")]
    public void GenerateLego()
    {
        if (string.IsNullOrEmpty(currentImageBase64))
        {
            UpdateStatus("Cần có ảnh trước khi tạo LEGO!");
            return;
        }

        StartCoroutine(GenerateLegoFlow());
    }

    [TabGroup("Actions")]
    [Button("🧱 Tạo LEGO từ Base64", ButtonSizes.Large)]
    public void GenerateLegoFromBase64()
    {
        currentImageBase64 = imageBase;
        if (string.IsNullOrEmpty(imageBase))
        {
            UpdateStatus("Cần có ảnh trước khi tạo LEGO!");
            return;
        }

        StartCoroutine(GenerateLegoFlow());
    }

    [TabGroup("Actions")]
    [Button("🧱 Tạo LEGO từ Texture2D", ButtonSizes.Large)]
    public void GenerateLegoFromTexture2D()
    {
        byte[] imageData = image.EncodeToPNG();
        currentImageBase64 = Convert.ToBase64String(imageData);
        if (string.IsNullOrEmpty(imageBase))
        {
            UpdateStatus("Cần có ảnh trước khi tạo LEGO!");
            return;
        }

        StartCoroutine(GenerateLegoFlow());
    }

    [TabGroup("Actions")]
    [Button("📥 Tải File LDR", ButtonSizes.Medium)]
    [EnableIf("@currentLegoData != null && !string.IsNullOrEmpty(currentLegoData.model_url)")]
    public void DownloadLDRFile()
    {
        if (currentLegoData != null && !string.IsNullOrEmpty(currentLegoData.model_url))
        {
            StartCoroutine(DownloadLDRFlow());
        }
    }

    [TabGroup("Actions")]
    [Button("🔗 Tải LDR từ S3 URL", ButtonSizes.Medium)]
    [EnableIf("@!string.IsNullOrEmpty(s3LdrUrl)")]
    public void DownloadLDRFromS3Url()
    {
        if (string.IsNullOrEmpty(s3LdrUrl))
        {
            UpdateStatus("Vui lòng nhập S3 URL!");
            return;
        }

        DownloadS3LDRFlow();
    }

    [TabGroup("Actions")]
    [Button("📋 Clear Logs", ButtonSizes.Small)]
    public void ClearLogs()
    {
        logs.Clear();
        logMessages = "";
    }

    /// <summary>
    /// Flow tạo ảnh - nhận kết quả ngay lập tức
    /// </summary>
    private IEnumerator GenerateImageFlow()
    {
        UpdateStatus("🔄 Đang tạo ảnh từ prompt...");
        LogMessage($"Bắt đầu tạo ảnh với prompt: {imagePrompt}");

        yield return APIManager.Instance.CallGenImage(imagePrompt, (response, error) =>
        {
            if (error != null)
            {
                UpdateStatus($"❌ Lỗi tạo ảnh: {error}");
                LogMessage($"❌ Lỗi: {error}");
                return;
            }

            if (response != null && !string.IsNullOrEmpty(response.image))
            {
                // Chuyển đổi base64 thành texture và hiển thị
                try
                {
                    byte[] imageBytes = Convert.FromBase64String(response.image);
                    Texture2D texture = new Texture2D(2, 2);
                    texture.LoadImage(imageBytes);
                    
                    generatedImage = texture;
                    
                    // Lưu base64 để sử dụng cho LEGO
                    currentImageBase64 = response.image;
                    
                    UpdateStatus("✅ Tạo ảnh thành công! Có thể tạo LEGO từ ảnh này.");
                    LogMessage($"✅ Tạo ảnh thành công! Request ID: {response.requestId}");
                    LogMessage($"📏 Kích thước ảnh: {texture.width}x{texture.height}");
                }
                catch (Exception e)
                {
                    UpdateStatus($"❌ Lỗi hiển thị ảnh: {e.Message}");
                    LogMessage($"❌ Lỗi hiển thị: {e.Message}");
                }
            }
            else
            {
                UpdateStatus("❌ Không nhận được dữ liệu ảnh từ server");
                LogMessage("❌ Response không chứa dữ liệu ảnh");
            }
        });
    }

    /// <summary>
    /// Flow tạo LEGO - async với Firebase listener
    /// </summary>
    private IEnumerator GenerateLegoFlow()
    {
        UpdateStatus("🔄 Đang gửi request tạo LEGO...");
        LogMessage($"🧱 Bắt đầu tạo LEGO với Details: {legoDetails}, ForegroundRatio: {foregroundRatio}");

        yield return APIManager.Instance.CallGenLego(currentImageBase64, (API.LegoModelData modelData, string error) =>
        {
            OnLegoCompleted(modelData, error);
        }, legoDetails, foregroundRatio);
    }

    /// <summary>
    /// Flow tải xuống file LDR
    /// </summary>
    private IEnumerator DownloadLDRFlow()
    {
        UpdateStatus("📥 Đang tải file LDR...");
        LogMessage($"📥 Bắt đầu tải file LDR: {currentLegoData.model_url}");

        yield return LDRDownloader.DownloadLDRFile(currentLegoData.model_url, (ldrContent, error) =>
        {
            if (error != null)
            {
                UpdateStatus($"❌ Lỗi tải file LDR: {error}");
                LogMessage($"❌ Lỗi tải LDR: {error}");
                return;
            }

            if (!string.IsNullOrEmpty(ldrContent))
            {
                // Lưu file LDR
                string fileName = LDRDownloader.GetFileNameFromS3Path(currentLegoData.model_url);
                string savedPath = LDRDownloader.SaveLDRToLocal(ldrContent, fileName);
                
                if (!string.IsNullOrEmpty(savedPath))
                {
                    UpdateStatus("✅ Tải và lưu file LDR thành công!");
                    LogMessage($"✅ File LDR đã lưu tại: {savedPath}");
                    LogMessage($"📄 Kích thước file: {ldrContent.Length} characters");
                }
                else
                {
                    UpdateStatus("❌ Lỗi lưu file LDR");
                    LogMessage("❌ Không thể lưu file LDR");
                }
            }
            else
            {
                UpdateStatus("❌ File LDR rỗng");
                LogMessage("❌ Nội dung file LDR rỗng");
            }
        });
    }

    /// <summary>
    /// Flow tải xuống file LDR từ S3 URL được nhập thủ công
    /// </summary>
    private void DownloadS3LDRFlow()
    {
        UpdateStatus("📥 Đang tải file LDR từ S3 URL...");
        LogMessage($"📥 Bắt đầu tải file LDR từ URL: {s3LdrUrl}");
        string userId = FirebaseAuthManager.Instance.GetCurrentUserId();
        LDRDownloader.DownloadLDRFileFromS3Path(userId, s3LdrUrl, (ldrContent, error) =>
        {
            if (error != null)
            {
                UpdateStatus($"❌ Lỗi tải file LDR từ S3: {error}");
                LogMessage($"❌ Lỗi tải LDR từ S3: {error}");
                return;
            }

            if (!string.IsNullOrEmpty(ldrContent))
            {
                // Lưu file LDR
                string fileName = LDRDownloader.GetFileNameFromS3Path(s3LdrUrl);
                string savedPath = LDRDownloader.SaveLDRToLocal(ldrContent, fileName);
                
                if (!string.IsNullOrEmpty(savedPath))
                {
                    UpdateStatus("✅ Tải và lưu file LDR từ S3 thành công!");
                    LogMessage($"✅ File LDR từ S3 đã lưu tại: {savedPath}");
                    LogMessage($"📄 Kích thước file: {ldrContent.Length} characters");
                    LogMessage($"📂 Tên file: {fileName}.ldr");
                }
                else
                {
                    UpdateStatus("❌ Lỗi lưu file LDR từ S3");
                    LogMessage("❌ Không thể lưu file LDR từ S3");
                }
            }
            else
            {
                UpdateStatus("❌ File LDR từ S3 rỗng");
                LogMessage("❌ Nội dung file LDR từ S3 rỗng");
            }
        });
    }




    /// <summary>
    /// Callback khi quá trình tạo LEGO hoàn thành (từ Firebase)
    /// </summary>
    private void OnLegoCompleted(API.LegoModelData modelData, string error)
    {
        if (error != null)
        {
            UpdateStatus($"❌ Lỗi tạo LEGO: {error}");
            LogMessage($"❌ Lỗi tạo LEGO: {error}");
            return;
        }

        if (modelData != null)
        {
            currentLegoData = modelData;
            currentRequestId = modelData.requestId;
            
            UpdateStatus("✅ Tạo LEGO thành công!");
            LogMessage($"✅ LEGO hoàn thành! Request ID: {modelData.requestId}");
            LogMessage($"🔗 Model URL: {modelData.model_url}");
            LogMessage($"📊 Status: {modelData.status}");
            
            if (!string.IsNullOrEmpty(modelData.thumbnail_url))
            {
                LogMessage($"🖼️ Thumbnail: {modelData.thumbnail_url}");
            }
            
            if (!string.IsNullOrEmpty(modelData.category))
            {
                LogMessage($"📂 Category: {modelData.category}");
            }
            
            if (!string.IsNullOrEmpty(modelData.polycount))
            {
                LogMessage($"🔢 Polycount: {modelData.polycount}");
            }
        }
        else
        {
            UpdateStatus("❌ Không nhận được dữ liệu LEGO");
            LogMessage("❌ Response không chứa dữ liệu LEGO");
        }
    }

    /// <summary>
    /// Cập nhật status
    /// </summary>
    private void UpdateStatus(string message)
    {
        currentStatus = message;
        Debug.Log($"[AIFlowDemo Status] {message}");
    }

    /// <summary>
    /// Thêm message vào log
    /// </summary>
    private void LogMessage(string message)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        string logEntry = $"[{timestamp}] {message}";
        
        logs.Add(logEntry);
        
        // Giới hạn số dòng log
        if (logs.Count > 50)
        {
            logs.RemoveAt(0);
        }
        
        // Cập nhật log text để hiển thị trong Inspector
        logMessages = string.Join("\n", logs);
        
        Debug.Log($"[AIFlowDemo Log] {message}");
    }

    void OnDestroy()
    {
        // Cleanup
        if (!string.IsNullOrEmpty(currentRequestId))
        {
            APIManager.Instance?.CancelLegoRequest(currentRequestId);
        }
    }
} 