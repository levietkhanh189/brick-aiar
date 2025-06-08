using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;

namespace UI
{
    public class Popup_AIImageToLego : DTNView
    {
        [SerializeField] private CanvasGroup canvasGroup;

        [SerializeField] private Button buttonClose;
        [SerializeField] private Button buttonLoadImage;
        [SerializeField] private Button buttonCraftLego;
        [SerializeField] private Slider details;
        [SerializeField] private Slider foregroundRatio;
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private Texture2D texture;
        [SerializeField] private RawImage image;

        public override void Init()
        {
            SetupButtons();
        }

        private void SetupButtons()
        {
            if (buttonClose != null)
                buttonClose.onClick.AddListener(OnCloseClicked);
            if (buttonLoadImage != null)
                buttonLoadImage.onClick.AddListener(OnLoadImageClicked);
            if (buttonCraftLego != null)
                buttonCraftLego.onClick.AddListener(OnCraftLegoClicked);
        }

        public override void Show()
        {
            base.Show();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
                canvasGroup.DOFade(1, fadeDuration);
            }
        }

        public override void Hide()
        {
            if (canvasGroup != null)
            {
                canvasGroup.DOFade(0, fadeDuration)
                    .OnComplete(() => base.Hide());
            }
            else
            {
                base.Hide();
            }
        }

        private void OnCloseClicked()
        {
            Hide();
        }

        private void OnLoadImageClicked()
        {
            Debug.Log("Load Image button clicked");
            
            // Sử dụng NativeFilePicker để chọn ảnh từ thiết bị
            #if UNITY_ANDROID || UNITY_IOS
                // Kiểm tra quyền trước khi mở file picker
                if (!NativeFilePicker.CheckPermission())
                {
                    NativeFilePicker.RequestPermissionAsync((permission) =>
                    {
                        if (permission == NativeFilePicker.Permission.Granted)
                        {
                            OpenImagePicker();
                        }
                        else
                        {
                            Debug.LogWarning("Không có quyền truy cập file system");
                        }
                    });
                }
                else
                {
                    OpenImagePicker();
                }
            #else
                // Trên editor hoặc các platform khác, sử dụng texture mặc định
                image.texture = texture;
                Debug.Log("Sử dụng texture mặc định trên editor");
            #endif
        }

        private void OpenImagePicker()
        {
            // Chỉ cho phép chọn file ảnh
            string[] allowedFileTypes = { "image/*" }; // Android format
            
            #if UNITY_IOS
                allowedFileTypes = new string[] { "public.image" }; // iOS format
            #endif

            NativeFilePicker.PickFile((filePath) =>
            {
                if (!string.IsNullOrEmpty(filePath))
                {
                    Debug.Log($"Đã chọn file: {filePath}");
                    StartCoroutine(LoadImageFromFile(filePath));
                }
                else
                {
                    Debug.Log("Người dùng đã hủy chọn file");
                }
            }, allowedFileTypes);
        }

        private IEnumerator LoadImageFromFile(string filePath)
        {
            // Đọc file và tạo texture
            if (System.IO.File.Exists(filePath))
            {
                byte[] fileData = System.IO.File.ReadAllBytes(filePath);
                
                // Tạo texture mới
                Texture2D loadedTexture = new Texture2D(2, 2);
                
                if (loadedTexture.LoadImage(fileData))
                {
                    // Cập nhật texture cho RawImage
                    image.texture = loadedTexture;
                    
                    // Cập nhật texture reference để sử dụng trong OnCraftLegoClicked
                    if (texture != null)
                    {
                        DestroyImmediate(texture);
                    }
                    texture = loadedTexture;
                    
                    Debug.Log($"Đã load thành công ảnh: {loadedTexture.width}x{loadedTexture.height}");
                }
                else
                {
                    Debug.LogError("Không thể load ảnh từ file");
                    DestroyImmediate(loadedTexture);
                }
            }
            else
            {
                Debug.LogError($"File không tồn tại: {filePath}");
            }
            
            yield return null;
        }

        private void OnCraftLegoClicked()
        {
            if (texture != null)
            {
                Debug.Log("Craft Lego button clicked");
                AIFlowController.Instance.CraftImageToLego(TextureToBase64.ConvertToBase64(texture), details.value, foregroundRatio.value);
                Hide();
            }
            else
            {
                Debug.LogWarning("Chưa có ảnh để tạo Lego");
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (buttonClose != null)
                buttonClose.onClick.RemoveListener(OnCloseClicked);
            if (buttonLoadImage != null)
                buttonLoadImage.onClick.RemoveListener(OnLoadImageClicked);
            if (buttonCraftLego != null)
                buttonCraftLego.onClick.RemoveListener(OnCraftLegoClicked);
                
            // Dọn dẹp texture đã tạo
            if (texture != null)
            {
                DestroyImmediate(texture);
            }
        }
    }
}