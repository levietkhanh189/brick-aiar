using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Sirenix.OdinInspector;

namespace UI
{
    public class Popup_CheckMail : DTNView
    {
        [FoldoutGroup("References")]
        [SerializeField] private CanvasGroup canvasGroup;

        [FoldoutGroup("References")]
        [SerializeField] private Button buttonClose;

        [FoldoutGroup("References")]
        [SerializeField] private Button buttonLater;

        [FoldoutGroup("References")]
        [SerializeField] private Button buttonUpdate;

        [FoldoutGroup("References")]
        [SerializeField] private TextMeshProUGUI textInfo;

        [FoldoutGroup("Config")]
        [SerializeField] private float fadeDuration = 0.5f;

        // Example of connecting to other scripts/managers
        // private SomeManager someManager;

        public override void Init()
        {
            // Initialize references to other scripts if needed
            // someManager = FindObjectOfType<SomeManager>();

            SetupButtons();
        }

        private void SetupButtons()
        {
            if (buttonClose != null)
                buttonClose.onClick.AddListener(OnCloseClicked);
            if (buttonLater != null)
                buttonLater.onClick.AddListener(OnLaterClicked);
            if (buttonUpdate != null)
                buttonUpdate.onClick.AddListener(OnUpdateClicked);
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

        private void OnCloseClicked()
        {
            Hide();
        }

        private void OnLaterClicked()
        {
            // Implement logic for "Later" button click
            Debug.Log("Button_Later clicked");
            Hide();
        }

        private void OnUpdateClicked()
        {
            // Implement logic for "Update" button click
            Debug.Log("Button_Update clicked");
            Hide();

            // Example: Notify some manager to start update process
            // someManager.StartUpdateProcess();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (buttonClose != null)
                buttonClose.onClick.RemoveListener(OnCloseClicked);
            if (buttonLater != null)
                buttonLater.onClick.RemoveListener(OnLaterClicked);
            if (buttonUpdate != null)
                buttonUpdate.onClick.RemoveListener(OnUpdateClicked);
        }
    }
}
