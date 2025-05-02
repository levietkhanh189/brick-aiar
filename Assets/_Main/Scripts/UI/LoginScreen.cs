using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Threading.Tasks;

namespace UI
{
    public class LoginScreen : DTNView
    {
        [FoldoutGroup("References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [FoldoutGroup("References")]
        [SerializeField] private TMP_InputField emailInput;
        [FoldoutGroup("References")]
        [SerializeField] private TMP_InputField passwordInput;
        [FoldoutGroup("References")]
        [SerializeField] private Button loginButton;
        [FoldoutGroup("References")]
        [SerializeField] private Button registerButton;
        [FoldoutGroup("References")]
        [SerializeField] private Button googleLoginButton;
        [FoldoutGroup("References")]
        [SerializeField] private Button forgotPasswordButton;
        [FoldoutGroup("References")]
        [SerializeField] private TextMeshProUGUI errorText;

        [FoldoutGroup("Config")]
        [SerializeField] private float fadeDuration = 0.5f;
        [FoldoutGroup("Config")]
        [SerializeField] private float errorDisplayDuration = 3f;
        private LoadingScreen loadingScreen;
        private FirebaseAuthManager authManager;

        // Initialize references and setup button listeners
        public override void Init()
        {
            authManager = FindObjectOfType<FirebaseAuthManager>();
            loadingScreen = DTNWindow.FindTopWindow().GetView<LoadingScreen>();

            if (authManager == null)
            {
                Debug.LogError("FirebaseAuthManager was not found in the scene!");
                return;
            }

            SetupButtons();
        }

        // Assign button click listeners
        private void SetupButtons()
        {
            loginButton.onClick.AddListener(OnLoginClicked);
            registerButton.onClick.AddListener(OnRegisterClicked);
            googleLoginButton.onClick.AddListener(OnGoogleLoginClicked);
            forgotPasswordButton.onClick.AddListener(OnForgotPasswordClicked);
        }

        // Show the login screen with fade-in effect and clear input fields
        public override void Show()
        {
            base.Show();
            canvasGroup.alpha = 0;
            canvasGroup.DOFade(1, fadeDuration);
            ClearInputs();
        }

        // Hide the login screen with fade-out effect
        public override void Hide()
        {
            canvasGroup.DOFade(0, fadeDuration)
                .OnComplete(() => base.Hide());
        }

        // Clear input fields and error text
        private void ClearInputs()
        {
            emailInput.text = string.Empty;
            passwordInput.text = string.Empty;
            errorText.text = string.Empty;
        }

        // Handle login button click
        private async void OnLoginClicked()
        {
            if (!ValidateInputs()) return;
            loadingScreen.Show();
            loadingScreen.SetProgress(0.3f);

            try
            {
                await authManager.SignIn();
                loadingScreen.SetProgress(1f);
                Hide();
            }
            catch (System.Exception ex)
            {
                ShowError(ex.Message);
            }
            finally
            {
                loadingScreen.Hide();
            }
        }

        // Handle register button click
        private async void OnRegisterClicked()
        {
            if (!ValidateInputs()) return;

            loadingScreen.Show();
            loadingScreen.SetProgress(0.3f);

            try
            {
                await authManager.RegisterUser();
                loadingScreen.SetProgress(1f);
                ShowError("Registration successful! Please check your email to verify your account.");
            }
            catch (System.Exception ex)
            {
                ShowError(ex.Message);
            }
            finally
            {
                loadingScreen.Hide();
            }
        }

        // Handle Google login button click
        private async void OnGoogleLoginClicked()
        {
            loadingScreen.Show();
            loadingScreen.SetProgress(0.3f);

            try
            {
                // TODO: Implement Google Sign-In logic here
                // This is where you'll get the Google ID Token and Access Token
                // Then call authManager.SignInWithGoogle()
                ShowError("Google login feature is under development.");
            }
            catch (System.Exception ex)
            {
                ShowError(ex.Message);
            }
            finally
            {
                loadingScreen.Hide();
            }
        }

        // Handle forgot password button click
        private async void OnForgotPasswordClicked()
        {
            if (string.IsNullOrEmpty(emailInput.text))
            {
                ShowError("Please enter your email to reset your password.");
                return;
            }

            loadingScreen.Show();
            loadingScreen.SetProgress(0.3f);

            try
            {
                await authManager.ResetPassword();
                loadingScreen.SetProgress(1f);
                ShowError("Password reset email has been sent. Please check your inbox.");
            }
            catch (System.Exception ex)
            {
                ShowError(ex.Message);
            }
            finally
            {
                loadingScreen.Hide();
            }
        }

        // Validate email and password input fields
        private bool ValidateInputs()
        {
            if (string.IsNullOrEmpty(emailInput.text))
            {
                ShowError("Please enter your email.");
                return false;
            }

            if (string.IsNullOrEmpty(passwordInput.text))
            {
                ShowError("Please enter your password.");
                return false;
            }

            return true;
        }

        // Display error message with fade-in and fade-out effect
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

        // Remove button listeners on destroy
        public override void OnDestroy()
        {
            base.OnDestroy();
            loginButton.onClick.RemoveListener(OnLoginClicked);
            registerButton.onClick.RemoveListener(OnRegisterClicked);
            googleLoginButton.onClick.RemoveListener(OnGoogleLoginClicked);
            forgotPasswordButton.onClick.RemoveListener(OnForgotPasswordClicked);
        }
    }
} 