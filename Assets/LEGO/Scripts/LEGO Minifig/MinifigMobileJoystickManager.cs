using UnityEngine;
using UnityEngine.UI;
using Unity.LEGO.Minifig;

namespace Unity.LEGO.Minifig
{
    /// <summary>
    /// Component tự động tạo và quản lý joystick cho Minifig trên mobile
    /// </summary>
    public class MinifigMobileJoystickManager : MonoBehaviour
    {
        [Header("Joystick Settings")]
        [SerializeField] private GameObject joystickPrefab;
        [SerializeField] private bool autoCreateOnMobile = true;
        [SerializeField] private Vector2 joystickPosition = new Vector2(150, 150);
        [SerializeField] private float joystickSensitivity = 1.0f;
        
        [Header("References")]
        private MinifigController minifigController;
        private GameObject joystickInstance;
        private Joystick joystick;
        private Canvas uiCanvas;
        
        // Input từ joystick
        private Vector2 joystickInput;
        private bool isJoystickActive = false;

        void Awake()
        {
            // Lấy reference đến MinifigController
            minifigController = GetComponent<MinifigController>();
            if (minifigController == null)
            {
                Debug.LogError("MinifigMobileJoystickManager: Không tìm thấy MinifigController trên GameObject này!");
                enabled = false;
                return;
            }
        }

        void Start()
        {
            // Kiểm tra nếu đang chạy trên mobile và auto create được bật
            if (autoCreateOnMobile && IsMobile())
            {
                CreateJoystick();
            }
        }

        void Update()
        {
            if (isJoystickActive && joystick != null)
            {
                // Lấy input từ joystick
                joystickInput = joystick.Direction;
                
                // Áp dụng input vào MinifigController (sẽ cần modify MinifigController để nhận input từ external source)
                HandleJoystickInput();
            }
        }

        /// <summary>
        /// Tạo joystick trên UI Canvas
        /// </summary>
        public void CreateJoystick()
        {
            if (joystickInstance != null)
            {
                Debug.LogWarning("MinifigMobileJoystickManager: Joystick đã tồn tại!");
                return;
            }

            // Tìm hoặc tạo Canvas
            FindOrCreateCanvas();
            
            if (uiCanvas == null)
            {
                Debug.LogError("MinifigMobileJoystickManager: Không thể tạo hoặc tìm thấy Canvas!");
                return;
            }

            // Tạo joystick prefab
            if (joystickPrefab != null)
            {
                joystickInstance = Instantiate(joystickPrefab, uiCanvas.transform);
            }
            else
            {
                // Tạo joystick mặc định nếu không có prefab
                CreateDefaultJoystick();
            }

            if (joystickInstance != null)
            {
                // Thiết lập vị trí joystick
                RectTransform rectTransform = joystickInstance.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchorMin = Vector2.zero;
                    rectTransform.anchorMax = Vector2.zero;
                    rectTransform.anchoredPosition = joystickPosition;
                }

                // Lấy component Joystick
                joystick = joystickInstance.GetComponent<Joystick>();
                if (joystick != null)
                {
                    isJoystickActive = true;
                    Debug.Log("MinifigMobileJoystickManager: Joystick đã được tạo thành công!");
                }
                else
                {
                    Debug.LogError("MinifigMobileJoystickManager: Không tìm thấy component Joystick trên prefab!");
                }
            }
        }

        /// <summary>
        /// Xoá joystick
        /// </summary>
        public void DestroyJoystick()
        {
            if (joystickInstance != null)
            {
                DestroyImmediate(joystickInstance);
                joystickInstance = null;
                joystick = null;
                isJoystickActive = false;
                Debug.Log("MinifigMobileJoystickManager: Joystick đã được xoá!");
            }
        }

        /// <summary>
        /// Xử lý input từ joystick
        /// </summary>
        private void HandleJoystickInput()
        {
            if (minifigController == null || joystickInput.magnitude < 0.1f)
                return;

            // Chuyển đổi input joystick thành direction cho character
            Vector3 moveDirection = new Vector3(joystickInput.x, 0, joystickInput.y);
            moveDirection = Camera.main.transform.TransformDirection(moveDirection);
            moveDirection.y = 0;
            moveDirection.Normalize();

            // Áp dụng sensitivity
            moveDirection *= joystickSensitivity;

            // TODO: Cần modify MinifigController để có thể nhận input từ external source
            // Hiện tại MinifigController sử dụng Input.GetAxis nội bộ
            // Có thể cần thêm method SetExternalInput(Vector3 direction) vào MinifigController
        }

        /// <summary>
        /// Tìm hoặc tạo Canvas cho UI
        /// </summary>
        private void FindOrCreateCanvas()
        {
            // Tìm Canvas hiện có
            uiCanvas = FindObjectOfType<Canvas>();
            
            if (uiCanvas == null)
            {
                // Tạo Canvas mới
                GameObject canvasGO = new GameObject("Mobile UI Canvas");
                uiCanvas = canvasGO.AddComponent<Canvas>();
                uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                uiCanvas.sortingOrder = 100;

                // Thêm CanvasScaler
                CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;

                // Thêm GraphicRaycaster
                canvasGO.AddComponent<GraphicRaycaster>();
                
                Debug.Log("MinifigMobileJoystickManager: Đã tạo Canvas mới cho Mobile UI");
            }
        }

        /// <summary>
        /// Tạo joystick mặc định nếu không có prefab
        /// </summary>
        private void CreateDefaultJoystick()
        {
            // Tạo GameObject chính cho joystick
            GameObject joystickGO = new GameObject("Default Mobile Joystick");
            joystickGO.transform.SetParent(uiCanvas.transform, false);

            // Thêm RectTransform
            RectTransform rectTransform = joystickGO.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 200);

            // Thêm component DynamicJoystick
            DynamicJoystick dynamicJoystick = joystickGO.AddComponent<DynamicJoystick>();

            // Tạo background
            GameObject background = new GameObject("Background");
            background.transform.SetParent(joystickGO.transform, false);
            
            RectTransform bgRect = background.AddComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(200, 200);
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            Image bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(1, 1, 1, 0.3f);
            bgImage.raycastTarget = true;

            // Tạo handle
            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(background.transform, false);
            
            RectTransform handleRect = handle.AddComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(80, 80);
            handleRect.anchoredPosition = Vector2.zero;

            Image handleImage = handle.AddComponent<Image>();
            handleImage.color = new Color(1, 1, 1, 0.6f);

            // Gán references cho DynamicJoystick thông qua reflection hoặc public fields
            var backgroundField = typeof(Joystick).GetField("background", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (backgroundField != null)
                backgroundField.SetValue(dynamicJoystick, bgRect);

            var handleField = typeof(Joystick).GetField("handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (handleField != null)
                handleField.SetValue(dynamicJoystick, handleRect);

            joystickInstance = joystickGO;
            Debug.Log("MinifigMobileJoystickManager: Đã tạo joystick mặc định");
        }

        /// <summary>
        /// Kiểm tra xem có đang chạy trên mobile không
        /// </summary>
        private bool IsMobile()
        {
            return Application.isMobilePlatform || 
                   Application.platform == RuntimePlatform.Android || 
                   Application.platform == RuntimePlatform.IPhonePlayer;
        }

        /// <summary>
        /// Public method để bật/tắt joystick
        /// </summary>
        public void SetJoystickActive(bool active)
        {
            if (active && joystickInstance == null)
            {
                CreateJoystick();
            }
            else if (!active && joystickInstance != null)
            {
                DestroyJoystick();
            }
        }

        /// <summary>
        /// Lấy input hiện tại từ joystick
        /// </summary>
        public Vector2 GetJoystickInput()
        {
            return isJoystickActive && joystick != null ? joystick.Direction : Vector2.zero;
        }

        /// <summary>
        /// Thiết lập vị trí joystick
        /// </summary>
        public void SetJoystickPosition(Vector2 position)
        {
            joystickPosition = position;
            if (joystickInstance != null)
            {
                RectTransform rectTransform = joystickInstance.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = joystickPosition;
                }
            }
        }

        void OnDestroy()
        {
            // Tự động xoá joystick khi Minifig bị destroy
            DestroyJoystick();
        }

        void OnApplicationPause(bool pauseStatus)
        {
            // Xử lý khi app bị pause/resume
            if (pauseStatus)
            {
                if (joystickInstance != null)
                {
                    joystickInstance.SetActive(false);
                }
            }
            else
            {
                if (joystickInstance != null)
                {
                    joystickInstance.SetActive(true);
                }
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            // Validation trong editor
            if (joystickSensitivity < 0.1f)
                joystickSensitivity = 0.1f;
            if (joystickSensitivity > 5.0f)
                joystickSensitivity = 5.0f;
        }
#endif
    }
} 