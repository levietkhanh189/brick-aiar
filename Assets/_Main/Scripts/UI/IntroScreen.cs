using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Threading.Tasks;
using System.Collections;

namespace UI
{
    public class IntroScreen : DTNView
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Button buttonLoginWithEmail;
        [SerializeField] private Button buttonLoginWithGoogle;

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
                    StartCoroutine(AutoLogin(email, password));
                }
            }
        }

        private IEnumerator AutoLogin(string email, string password)
        {
            yield return null;
            var loadingScreen = DTNWindow.FindTopWindow().ShowSubView<LoadingScreen>();
            loadingScreen.InitIfNeed();
            loadingScreen.Show();
            loadingScreen.SetProgress(0.3f);

            yield return authManager.SignIn(email, password);
            yield return DataController.Instance.LoadUserAsync();

            loadingScreen.SetProgress(1f);
            Hide();
            GameSceneManager.Instance.LoadScene("MainScene");
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

            await authManager.SignInWithGooglePublic();
            loadingScreen.SetProgress(1f);
            Hide();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            buttonLoginWithEmail.onClick.RemoveListener(OnLoginWithEmailClicked);
            buttonLoginWithGoogle.onClick.RemoveListener(OnLoginWithGoogleClicked);
        }
    }
}
