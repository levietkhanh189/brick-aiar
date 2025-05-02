using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Threading.Tasks;

namespace UI
{
    public class Popup_SignIn : DTNView
    {
        [FoldoutGroup("References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [FoldoutGroup("References")]
        [SerializeField] private TMP_InputField usernameInput;
        [FoldoutGroup("References")]
        [SerializeField] private TMP_InputField passwordInput;
        [FoldoutGroup("References")]
        [SerializeField] private Button signInButton;
        [FoldoutGroup("References")]
        [SerializeField] private Button closeButton;
        [FoldoutGroup("References")]
        [SerializeField] private Button forgotPasswordButton;
        [FoldoutGroup("References")]
        [SerializeField] private Button signUpButton;
        [FoldoutGroup("References")]
        [SerializeField] private Toggle rememberMeToggle;
        [FoldoutGroup("References")]
        [SerializeField] private TextMeshProUGUI errorText;

        [FoldoutGroup("Config")]
        [SerializeField] private float fadeDuration = 0.5f;
        [FoldoutGroup("Config")]
        [SerializeField] private float errorDisplayDuration = 3f;

        private LoadingScreen loadingScreen;
        private FirebaseAuthManager authManager;
        private Popup_SignUp popupSignUp;

        public override void Init()
        {
            authManager = FindObjectOfType<FirebaseAuthManager>();
            loadingScreen = DTNWindow.FindTopWindow().GetView<LoadingScreen>();
            popupSignUp = DTNWindow.FindTopWindow().GetView<Popup_SignUp>();

            if (authManager == null)
            {
                Debug.LogError("FirebaseAuthManager was not found in the scene!");
                return;
            }

            if (popupSignUp == null)
            {
                Debug.LogError("Popup_SignUp was not found in the scene!");
                return;
            }

            SetupButtons();
        }

        private void SetupButtons()
        {
            signInButton.onClick.AddListener(OnSignInClicked);
            closeButton.onClick.AddListener(OnCloseClicked);
            forgotPasswordButton.onClick.AddListener(OnForgotPasswordClicked);
            signUpButton.onClick.AddListener(OnSignUpButtonClicked);
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
            usernameInput.text = string.Empty;
            passwordInput.text = string.Empty;
            errorText.text = string.Empty;
            if (rememberMeToggle != null)
                rememberMeToggle.isOn = false;
        }

        private async void OnSignInClicked()
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

        private void OnCloseClicked()
        {
            Hide();
        }

        private async void OnForgotPasswordClicked()
        {
            if (string.IsNullOrEmpty(usernameInput.text))
            {
                ShowError("Please enter your username or email to reset your password.");
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

        private void OnSignUpButtonClicked()
        {
            Hide();
            popupSignUp.Show();
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrEmpty(usernameInput.text))
            {
                ShowError("Please enter your username or email.");
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
            signInButton.onClick.RemoveListener(OnSignInClicked);
            closeButton.onClick.RemoveListener(OnCloseClicked);
            forgotPasswordButton.onClick.RemoveListener(OnForgotPasswordClicked);
            signUpButton.onClick.RemoveListener(OnSignUpButtonClicked);
        }
    }
}
