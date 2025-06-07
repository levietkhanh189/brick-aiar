using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace UI
{
    public class LoadingScreen : DTNView
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image progressBar;
        [SerializeField] private TextMeshProUGUI loadingText;
        [SerializeField] private TextMeshProUGUI progressText;

        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private string[] loadingMessages;
        [SerializeField] private float messageChangeInterval = 2f;

        private float currentProgress;
        private float targetProgress;
        private Tween progressTween;
        private Tween messageTween;

        /// <summary>
        /// Initialize the loading screen and hide it by default.
        /// </summary>
        public override void Init()
        {
            DontDestroyOnLoad(gameObject);
            Hide();
        }

        /// <summary>
        /// Show the loading screen with fade-in effect and start loading messages.
        /// </summary>
        public override void Show()
        {
            base.Show();
            canvasGroup.alpha = 0;
            canvasGroup.DOFade(1, fadeDuration);
            StartLoadingMessages();
        }

        /// <summary>
        /// Hide the loading screen with fade-out effect and stop loading messages.
        /// </summary>
        public override void Hide()
        {
            canvasGroup.DOFade(0, fadeDuration)
                .OnComplete(() => base.Hide());
            StopLoadingMessages();
        }

        /// <summary>
        /// Set the progress value for the loading bar.
        /// </summary>
        /// <param name="progress">Progress value between 0 and 1.</param>
        public void SetProgress(float progress)
        {
            targetProgress = Mathf.Clamp01(progress);
            progressTween?.Kill();
            progressTween = DOTween.To(() => currentProgress, x => currentProgress = x, targetProgress, 0.3f)
                .OnUpdate(() => UpdateProgressUI());
        }

        /// <summary>
        /// Update the progress bar and progress text UI.
        /// </summary>
        private void UpdateProgressUI()
        {
            progressBar.fillAmount = currentProgress;
            progressText.text = $"{Mathf.RoundToInt(currentProgress * 100)}%";
        }

        /// <summary>
        /// Start cycling through loading messages at a set interval.
        /// </summary>
        private void StartLoadingMessages()
        {
            if (loadingMessages == null || loadingMessages.Length == 0) return;

            int currentIndex = 0;
            loadingText.text = loadingMessages[currentIndex];

            messageTween = DOTween.Sequence()
                .AppendInterval(messageChangeInterval)
                .AppendCallback(() =>
                {
                    currentIndex = (currentIndex + 1) % loadingMessages.Length;
                    loadingText.text = loadingMessages[currentIndex];
                })
                .SetLoops(-1);
        }

        /// <summary>
        /// Stop cycling loading messages.
        /// </summary>
        private void StopLoadingMessages()
        {
            if (messageTween != null)
            {
                messageTween.Kill();
                messageTween = null;
            }
        }

        /// <summary>
        /// Clean up tweens when the object is destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();
            progressTween?.Kill();
            messageTween?.Kill();
        }
    }
}