using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Sirenix.OdinInspector;

namespace UI
{
    public class Popup_CraftWait : DTNView
    {
        [SerializeField] private CanvasGroup canvasGroup;

        [SerializeField] private Button buttonClose;
        [SerializeField] private Button buttonLater;
        [SerializeField] private Button buttonUpdate;

        [SerializeField] private TextMeshProUGUI textInfo;

        [SerializeField] private float fadeDuration = 0.5f;


        public override void Init()
        {
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
            Hide();
        }

        private void OnUpdateClicked()
        {
            Hide();
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
