using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Threading.Tasks;

namespace UI
{
    public class Popup_SignUp : DTNView
    {
        [FoldoutGroup("References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [FoldoutGroup("References")]
        [SerializeField] private TMP_InputField emailInput;
        [FoldoutGroup("References")]
        [SerializeField] private TMP_InputField usernameInput;
        [FoldoutGroup("References")]
        [SerializeField] private TMP_InputField passwordInput;
        [FoldoutGroup("References")]
        [SerializeField] private Button signUpButton;
        [FoldoutGroup("References")]
        [SerializeField] private Button closeButton;
        [FoldoutGroup("References")]
        [SerializeField] private Button showPasswordButton;
        [FoldoutGroup("References")]
        [SerializeField] private Button backToSignInButton;
        [FoldoutGroup("References")]
        [SerializeField] private TextMeshProUGUI errorText;

        [FoldoutGroup("Config")]
        [SerializeField] private float fadeDuration = 0.5f;
        [FoldoutGroup("Config")]
        [SerializeField] private float errorDisplayDuration = 3f;
        private FirebaseAuthManager authManager;
        private DataController dataController;
        private bool isPasswordVisible = false;

        public override void Init()
        {
            authManager = FindObjectOfType<FirebaseAuthManager>();
            dataController = FindObjectOfType<DataController>();

            if (authManager == null)
            {
                Debug.LogError("FirebaseAuthManager was not found in the scene!");
                return;
            }

            if (dataController == null)
            {
                Debug.LogError("DataController was not found in the scene!");
                return;
            }

            SetupButtons();
        }

        private void SetupButtons()
        {
            signUpButton.onClick.AddListener(OnSignUpClicked);
            closeButton.onClick.AddListener(OnCloseClicked);
            if (showPasswordButton != null)
                showPasswordButton.onClick.AddListener(OnShowPasswordClicked);
            if (backToSignInButton != null)
                backToSignInButton.onClick.AddListener(OnBackToSignInClicked);
        }

        public override void Show()
        {
            base.Show();
            canvasGroup.alpha = 0;
            canvasGroup.DOFade(1, fadeDuration);
            ClearInputs();
        }

        public override void Hide()
        {
            canvasGroup.DOFade(0, fadeDuration)
                .OnComplete(() => base.Hide());
        }

        private void ClearInputs()
        {
            emailInput.text = string.Empty;
            usernameInput.text = string.Empty;
            passwordInput.text = string.Empty;
            errorText.text = string.Empty;
            if (isPasswordVisible)
                TogglePasswordVisibility(false);
        }

        private async void OnSignUpClicked()
        {
            if (!ValidateInputs()) return;
            var loadingScreen = DTNWindow.FindTopWindow().ShowSubView<LoadingScreen>();
            loadingScreen.InitIfNeed();
            loadingScreen.Show();
            loadingScreen.SetProgress(0.3f);

            try
            {
                await authManager.RegisterUser(emailInput.text, passwordInput.text, usernameInput.text);
                
                // Create user in Firestore
                await CreateUserInFirestore(usernameInput.text, emailInput.text);

                OnBackToSignInClicked();

                DTNWindow.FindTopWindow().ShowSubView<Popup_CheckMail>();
            }
            catch (System.Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private async Task CreateUserInFirestore(string username, string email)
        {
            try
            {
                // Create a new User object
                FirestoreManager.User user = new FirestoreManager.User
                {
                    uid = authManager.UserId,
                    userName = username,
                    email = email,
                    createdAt = Firebase.Firestore.Timestamp.FromDateTime(DateTime.UtcNow),
                    role = "user",
                    modelCount = 0,
                    isVerified = authManager.IsEmailVerified
                };

                // Save user to Firestore
                await dataController.CreateOrUpdateUserAsync(user);
                
                // Store user info in UserInfo singleton
                UserInfo.Instance.SetUserData(user);
                
                Debug.Log($"User created in Firestore: {username} ({authManager.UserId})");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to create user in Firestore: {ex.Message}");
                throw;
            }
        }

        private void OnCloseClicked()
        {
            Hide();
        }

        private void OnShowPasswordClicked()
        {
            TogglePasswordVisibility(!isPasswordVisible);
        }

        private void OnBackToSignInClicked()
        {
            Hide();
            var popupSignIn = DTNWindow.FindTopWindow().ShowSubView<Popup_SignIn>();
            popupSignIn.InitIfNeed();
            popupSignIn.Show();
        }

        private void TogglePasswordVisibility(bool visible)
        {
            isPasswordVisible = visible;
#if UNITY_2021_1_OR_NEWER
            passwordInput.contentType = visible ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
            passwordInput.ForceLabelUpdate();
#else
            passwordInput.contentType = visible ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
            passwordInput.ForceLabelUpdate();
#endif
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrEmpty(emailInput.text))
            {
                ShowError("Please enter your email.");
                return false;
            }
            if (string.IsNullOrEmpty(usernameInput.text))
            {
                ShowError("Please enter your username.");
                return false;
            }
            if (string.IsNullOrEmpty(passwordInput.text))
            {
                ShowError("Please enter your password.");
                return false;
            }
            return true;
        }

        private void ShowError(string message)
        {
            errorText.text = message;
            errorText.alpha = 0;
            DOTween.To(() => errorText.alpha, x => errorText.alpha = x, 1, 0.2f)
                .OnComplete(() =>
                {
                    DOVirtual.DelayedCall(errorDisplayDuration, () =>
                    {
                        DOTween.To(() => errorText.alpha, x => errorText.alpha = x, 0, 0.2f);
                    });
                });
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            signUpButton.onClick.RemoveListener(OnSignUpClicked);
            closeButton.onClick.RemoveListener(OnCloseClicked);
            if (showPasswordButton != null)
                showPasswordButton.onClick.RemoveListener(OnShowPasswordClicked);
            if (backToSignInButton != null)
                backToSignInButton.onClick.RemoveListener(OnBackToSignInClicked);
        }
    }
}
