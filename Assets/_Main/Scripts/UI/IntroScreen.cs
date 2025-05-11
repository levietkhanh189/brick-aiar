using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Threading.Tasks;

namespace UI
{
    public class IntroScreen : DTNView
    {
        [FoldoutGroup("References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [FoldoutGroup("References")]
        [SerializeField] private Button buttonLoginWithEmail;
        [FoldoutGroup("References")]
        [SerializeField] private Button buttonLoginWithGoogle;

        [FoldoutGroup("Config")]
        [SerializeField] private float fadeDuration = 0.5f;

        private FirebaseAuthManager authManager;
        private DataController dataController;
        public override void Init()
        {
            authManager = FindObjectOfType<FirebaseAuthManager>();
            dataController = FindObjectOfType<DataController>();
            if (authManager == null)
            {
                Debug.LogError("FirebaseAuthManager was not found in the scene!");
                return;
            }

            SetupButtons();
        }

        private void SetupButtons()
        {
            buttonLoginWithEmail.onClick.AddListener(OnLoginWithEmailClicked);
            buttonLoginWithGoogle.onClick.AddListener(OnLoginWithGoogleClicked);
        }

        public override void Show()
        {
            base.Show();
            canvasGroup.alpha = 0;
            canvasGroup.DOFade(1, fadeDuration);

            // Auto login nếu đã lưu
            if (PlayerPrefs.GetInt("AutoLogin_Enabled", 0) == 1)
            {
                string email = PlayerPrefs.GetString("AutoLogin_Email", "");
                string password = PlayerPrefs.GetString("AutoLogin_Password", "");
                if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
                {
                    AutoLogin(email, password);
                }
            }
        }

        private async void AutoLogin(string email, string password)
        {
            var loadingScreen = DTNWindow.FindTopWindow().ShowSubView<LoadingScreen>();
            loadingScreen.InitIfNeed();
            loadingScreen.Show();
            loadingScreen.SetProgress(0.3f);

            try
            {
                authManager.SetLoginInfo(email, password);
                await authManager.SignIn();
                authManager.UpdateUserInfo();
                FirestoreManager.User user = null;
                if (dataController != null)
                {
                    user = await dataController.LoadUserAsync(authManager.UserId);
                    UserInfo.Instance.SetUserData(user);
                }
                loadingScreen.SetProgress(1f);
                Hide();
                GameSceneManager.Instance.LoadScene("MainScene");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Auto login failed: {ex.Message}");
            }
            finally
            {
                loadingScreen.Hide();
            }
        }

        public override void Hide()
        {
            canvasGroup.DOFade(0, fadeDuration)
                .OnComplete(() => base.Hide());
        }

        private void OnLoginWithEmailClicked()
        {
            var popupSignIn = DTNWindow.FindTopWindow().ShowSubView<Popup_SignIn>();
            // Only keep reference if you need to interact after Show, otherwise just show
        }

        private async void OnLoginWithGoogleClicked()
        {
            var loadingScreen = DTNWindow.FindTopWindow().ShowSubView<LoadingScreen>();
            loadingScreen.InitIfNeed();
            loadingScreen.Show();
            loadingScreen.SetProgress(0.3f);

            try
            {
                await authManager.SignInWithGooglePublic();
                loadingScreen.SetProgress(1f);
                Hide();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Login with Google failed: {ex.Message}");
            }
            finally
            {
                loadingScreen.Hide();
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            buttonLoginWithEmail.onClick.RemoveListener(OnLoginWithEmailClicked);
            buttonLoginWithGoogle.onClick.RemoveListener(OnLoginWithGoogleClicked);
        }
    }
}
