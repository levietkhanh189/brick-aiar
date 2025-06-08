using UnityEngine;
using Unity.LEGO.Minifig;

namespace Unity.LEGO.Minifig
{
    /// <summary>
    /// Extension cho MinifigController để hỗ trợ input từ joystick và các nguồn external khác
    /// </summary>
    [RequireComponent(typeof(MinifigController))]
    public class MinifigControllerExtension : MonoBehaviour
    {
        [Header("External Input Settings")]
        [SerializeField] private bool useExternalInput = false;
        [SerializeField] private bool overrideKeyboardInput = true;
        [SerializeField] private float externalInputDeadZone = 0.1f;
        
        private MinifigController minifigController;
        private MinifigMobileJoystickManager joystickManager;
        
        // External input values
        private Vector2 externalMovementInput;
        private bool externalJumpInput;
        
        // Original Input methods để fallback
        private bool useOriginalInput = true;

        void Awake()
        {
            minifigController = GetComponent<MinifigController>();
            joystickManager = GetComponent<MinifigMobileJoystickManager>();
            
            if (minifigController == null)
            {
                Debug.LogError("MinifigControllerExtension: Không tìm thấy MinifigController!");
                enabled = false;
            }
        }

        void Start()
        {
            // Tự động bật external input nếu có joystick manager và đang ở mobile
            if (joystickManager != null && Application.isMobilePlatform)
            {
                SetUseExternalInput(true);
            }
        }

        void Update()
        {
            if (useExternalInput)
            {
                UpdateExternalInput();
                ApplyExternalInputToController();
            }
        }

        /// <summary>
        /// Cập nhật input từ các nguồn external
        /// </summary>
        private void UpdateExternalInput()
        {
            // Reset input values
            externalMovementInput = Vector2.zero;
            externalJumpInput = false;

            // Lấy input từ joystick manager nếu có
            if (joystickManager != null)
            {
                Vector2 joystickInput = joystickManager.GetJoystickInput();
                if (joystickInput.magnitude > externalInputDeadZone)
                {
                    externalMovementInput = joystickInput;
                }
            }

            // Có thể thêm các nguồn input khác ở đây (touch controls, gamepad, etc.)
            
            // Nếu không override keyboard input, combine với keyboard
            if (!overrideKeyboardInput)
            {
                float horizontal = Input.GetAxis("Horizontal");
                float vertical = Input.GetAxis("Vertical");
                Vector2 keyboardInput = new Vector2(horizontal, vertical);
                
                // Sử dụng input có magnitude lớn hơn
                if (keyboardInput.magnitude > externalMovementInput.magnitude)
                {
                    externalMovementInput = keyboardInput;
                }
            }

            // Jump input (có thể từ touch button hoặc keyboard)
            externalJumpInput = Input.GetButtonDown("Jump");
        }

        /// <summary>
        /// Áp dụng external input vào MinifigController
        /// </summary>
        private void ApplyExternalInputToController()
        {
            if (minifigController == null)
                return;

            // Sử dụng reflection để truy cập private/protected fields và methods
            // Hoặc có thể tạo custom movement logic dựa trên MinifigController
            
            // Đây là cách tiếp cận đơn giản - tạo movement logic tương tự
            SimulateMovementInput();
        }

        /// <summary>
        /// Mô phỏng movement input tương tự như trong MinifigController
        /// </summary>
        private void SimulateMovementInput()
        {
            if (externalMovementInput.magnitude < externalInputDeadZone)
                return;

            // Lấy camera direction để tính toán movement relative to camera
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
                return;

            // Chuyển đổi input 2D thành 3D movement direction
            Vector3 cameraForward = mainCamera.transform.forward;
            Vector3 cameraRight = mainCamera.transform.right;
            
            // Project onto horizontal plane
            cameraForward.y = 0;
            cameraRight.y = 0;
            cameraForward.Normalize();
            cameraRight.Normalize();

            // Tính movement direction relative to camera
            Vector3 moveDirection = cameraForward * externalMovementInput.y + cameraRight * externalMovementInput.x;
            moveDirection.Normalize();

            // Apply movement bằng cách sử dụng public methods của MinifigController nếu có
            // Hoặc manipulate transform trực tiếp (không recommended)
            ApplyMovement(moveDirection);
        }

        /// <summary>
        /// Áp dụng movement lên MinifigController
        /// </summary>
        private void ApplyMovement(Vector3 direction)
        {
            if (direction.magnitude < 0.1f)
                return;

            // Sử dụng MoveTo method của MinifigController để di chuyển
            Vector3 targetPosition = transform.position + direction * 0.1f; // Small step
            
            // Gọi MoveTo với parameters phù hợp
            try
            {
                // Sử dụng reflection để gọi method nếu cần
                var moveToMethod = typeof(MinifigController).GetMethod("MoveTo");
                if (moveToMethod != null)
                {
                    // MoveTo(Vector3 destination, float minDistance = 0.0f, ...)
                    moveToMethod.Invoke(minifigController, new object[] { 
                        targetPosition, 
                        0.0f, 
                        null, 
                        0.0f, 
                        0.0f, 
                        true, 
                        1.0f, 
                        1.0f, 
                        null 
                    });
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"MinifigControllerExtension: Không thể gọi MoveTo - {e.Message}");
                
                // Fallback: Direct transform manipulation (not ideal)
                FallbackMovement(direction);
            }
        }

        /// <summary>
        /// Fallback movement method khi không thể sử dụng MinifigController methods
        /// </summary>
        private void FallbackMovement(Vector3 direction)
        {
            // Simple transform-based movement as fallback
            float moveSpeed = 5.0f; // Should match minifigController.maxForwardSpeed
            Vector3 movement = direction * moveSpeed * Time.deltaTime;
            
            // Kiểm tra collision trước khi di chuyển
            CharacterController characterController = GetComponent<CharacterController>();
            if (characterController != null)
            {
                characterController.Move(movement);
            }
            else
            {
                transform.position += movement;
            }

            // Rotate toward movement direction
            if (direction.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5.0f);
            }
        }

        /// <summary>
        /// Bật/tắt sử dụng external input
        /// </summary>
        public void SetUseExternalInput(bool use)
        {
            useExternalInput = use;
            useOriginalInput = !use;
            
            if (use)
            {
                Debug.Log("MinifigControllerExtension: Đã bật external input");
            }
            else
            {
                Debug.Log("MinifigControllerExtension: Đã tắt external input");
            }
        }

        /// <summary>
        /// Thiết lập input movement từ external source
        /// </summary>
        public void SetExternalMovementInput(Vector2 input)
        {
            externalMovementInput = input;
        }

        /// <summary>
        /// Thiết lập jump input từ external source
        /// </summary>
        public void SetExternalJumpInput(bool jump)
        {
            externalJumpInput = jump;
        }

        /// <summary>
        /// Lấy trạng thái hiện tại của external input
        /// </summary>
        public bool IsUsingExternalInput()
        {
            return useExternalInput;
        }

        /// <summary>
        /// Lấy external movement input hiện tại
        /// </summary>
        public Vector2 GetExternalMovementInput()
        {
            return externalMovementInput;
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (externalInputDeadZone < 0.01f)
                externalInputDeadZone = 0.01f;
            if (externalInputDeadZone > 0.5f)
                externalInputDeadZone = 0.5f;
        }

        // Draw gizmos để debug movement direction
        void OnDrawGizmosSelected()
        {
            if (useExternalInput && externalMovementInput.magnitude > externalInputDeadZone)
            {
                Gizmos.color = Color.green;
                Vector3 direction = new Vector3(externalMovementInput.x, 0, externalMovementInput.y);
                if (Camera.main != null)
                {
                    Vector3 cameraForward = Camera.main.transform.forward;
                    Vector3 cameraRight = Camera.main.transform.right;
                    cameraForward.y = 0;
                    cameraRight.y = 0;
                    cameraForward.Normalize();
                    cameraRight.Normalize();
                    direction = cameraForward * externalMovementInput.y + cameraRight * externalMovementInput.x;
                }
                
                Gizmos.DrawRay(transform.position, direction * 2.0f);
                Gizmos.DrawWireSphere(transform.position + direction * 2.0f, 0.2f);
            }
        }
#endif
    }
} 