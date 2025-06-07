using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using API;

namespace UI
{
    public class Popup_AITextToLego : DTNView
    {
        [SerializeField] private CanvasGroup canvasGroup;

        [SerializeField] private Button buttonClose;
        [SerializeField] private TMP_InputField promt;
        [SerializeField] private Button buttonCraftLego;
        [SerializeField] private Slider details;
        [SerializeField] private Slider foregroundRatio;
        [SerializeField] private float fadeDuration = 0.5f;

        public override void Init()
        {
            SetupButtons();
        }

        private void SetupButtons()
        {
            if (buttonClose != null)
                buttonClose.onClick.AddListener(OnCloseClicked);
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

        private void OnCraftLegoClicked()
        {
            Debug.Log("Text To Lego button clicked");
            AIFlowController.Instance.CraftTextToLego(promt.text, details.value, foregroundRatio.value);
            Hide();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (buttonClose != null)
                buttonClose.onClick.RemoveListener(OnCloseClicked);
            if (buttonCraftLego != null)
                buttonCraftLego.onClick.RemoveListener(OnCraftLegoClicked);
        }
    }
}