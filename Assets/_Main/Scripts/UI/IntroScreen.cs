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
        private LoadingScreen loadingScreen;
        private Popup_SignIn popupSignIn;

        public override void Init()
        {
            authManager = FindObjectOfType<FirebaseAuthManager>();
            loadingScreen = DTNWindow.FindTopWindow().GetView<LoadingScreen>();
            popupSignIn = DTNWindow.FindTopWindow().GetView<Popup_SignIn>();

            if (authManager == null)
            {
                Debug.LogError("FirebaseAuthManager was not found in the scene!");
                return;
            }

            if (popupSignIn == null)
            {
                Debug.LogError("Popup_SignIn was not found in the scene!");
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
        }

        public override void Hide()
        {
            canvasGroup.DOFade(0, fadeDuration)
                .OnComplete(() => base.Hide());
        }

        private void OnLoginWithEmailClicked()
        {
            popupSignIn.Show();
        }

        private async void OnLoginWithGoogleClicked()
        {
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
