using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Sirenix.OdinInspector;

namespace UI
{
    public class Popup_AIImageToLego : DTNView
    {
        [SerializeField] private CanvasGroup canvasGroup;

        [SerializeField] private Button buttonClose;
        [SerializeField] private Button buttonLoadImage;
        [SerializeField] private Button buttonCraftLego;
        [SerializeField] private Slider details;
        [SerializeField] private Slider foregroundRatio;
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private Texture2D texture;
        [SerializeField] private RawImage image;
        public override void Init()
        {
            SetupButtons();
        }

        private void SetupButtons()
        {
            if (buttonClose != null)
                buttonClose.onClick.AddListener(OnCloseClicked);
            if (buttonLoadImage != null)
                buttonLoadImage.onClick.AddListener(OnLoadImageClicked);
            if (buttonCraftLego != null)
                buttonCraftLego.onClick.AddListener(OnCraftLegoClicked);
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

        private void OnLoadImageClicked()
        {
            Debug.Log("Image To Lego button clicked");
            image.texture = texture;
        }

        private void OnCraftLegoClicked()
        {
            Debug.Log("Text To Lego button clicked");
            AIFlowController.Instance.CraftImageToLego(TextureToBase64.ConvertToBase64(texture), details.value, foregroundRatio.value);
            Hide();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (buttonClose != null)
                buttonClose.onClick.RemoveListener(OnCloseClicked);
            if (buttonLoadImage != null)
                buttonLoadImage.onClick.RemoveListener(OnLoadImageClicked);
            if (buttonCraftLego != null)
                buttonCraftLego.onClick.RemoveListener(OnCraftLegoClicked);
        }
    }
}