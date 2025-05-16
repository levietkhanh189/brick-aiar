using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Sirenix.OdinInspector;

namespace UI
{
    public class Popup_AICraftOption : DTNView
    {
        [FoldoutGroup("References")]
        [SerializeField] private CanvasGroup canvasGroup;

        [FoldoutGroup("References")]
        [SerializeField] private Button buttonClose;
        [FoldoutGroup("References")]
        [SerializeField] private Button buttonImageToLego;
        [FoldoutGroup("References")]
        [SerializeField] private Button buttonTextToLego;

        [FoldoutGroup("Config")]
        [SerializeField] private float fadeDuration = 0.5f;

        public override void Init()
        {
            // someManager = FindObjectOfType<SomeManager>();
            SetupButtons();
        }

        private void SetupButtons()
        {
            if (buttonClose != null)
                buttonClose.onClick.AddListener(OnCloseClicked);
            if (buttonImageToLego != null)
                buttonImageToLego.onClick.AddListener(OnImageToLegoClicked);
            if (buttonTextToLego != null)
                buttonTextToLego.onClick.AddListener(OnTextToLegoClicked);
        }

        public override void Show()
        {
            base.Show();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
                canvasGroup.DOFade(1, fadeDuration);
            }
        }

        public override void Hide()
        {
            if (canvasGroup != null)
            {
                canvasGroup.DOFade(0, fadeDuration)
                    .OnComplete(() => base.Hide());
            }
            else
            {
                base.Hide();
            }
        }

        private void OnCloseClicked()
        {
            Hide();
        }

        private void OnImageToLegoClicked()
        {
            Debug.Log("Image To Lego button clicked");
            // TODO: Connect to image-to-lego workflow or manager
            // Example: someManager.StartImageToLego();
            Hide();
        }

        private void OnTextToLegoClicked()
        {
            Debug.Log("Text To Lego button clicked");
            // TODO: Connect to text-to-lego workflow or manager
            // Example: someManager.StartTextToLego();
            Hide();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (buttonClose != null)
                buttonClose.onClick.RemoveListener(OnCloseClicked);
            if (buttonImageToLego != null)
                buttonImageToLego.onClick.RemoveListener(OnImageToLegoClicked);
            if (buttonTextToLego != null)
                buttonTextToLego.onClick.RemoveListener(OnTextToLegoClicked);
        }
    }
}
