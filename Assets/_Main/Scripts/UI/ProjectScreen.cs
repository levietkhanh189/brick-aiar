using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace UI
{
    public class ProjectScreen : DTNView
    {
        [SerializeField] private CanvasGroup canvasGroup;

        [SerializeField] private Button buttonClose;
        [SerializeField] private Button buttonProjectLego3D;
        [SerializeField] private Button buttonProjectLegoAR;
        [SerializeField] private Button buttonLegoSample;


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
            if (buttonProjectLego3D != null)
                buttonProjectLego3D.onClick.AddListener(OnProjectLego3DClicked);
            if (buttonProjectLegoAR != null)
                buttonProjectLegoAR.onClick.AddListener(OnProjectLegoARClicked);
            if (buttonLegoSample != null)
                buttonLegoSample.onClick.AddListener(OnLegoSampleClicked);
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

        private void OnProjectLego3DClicked()
        {
            GameSceneManager.Instance.LoadScene("ProjectScene3D");
        }

        private void OnProjectLegoARClicked()
        {
            GameSceneManager.Instance.LoadScene("ProjectSceneAR");
        }

        private void OnLegoSampleClicked()
        {
            GameSceneManager.Instance.LoadScene("ProjectSceneSample");
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (buttonClose != null)
                buttonClose.onClick.RemoveListener(OnCloseClicked);
            if (buttonProjectLego3D != null)
                buttonProjectLego3D.onClick.RemoveListener(OnProjectLego3DClicked);
            if (buttonProjectLegoAR != null)
                buttonProjectLegoAR.onClick.RemoveListener(OnProjectLegoARClicked);
            if (buttonLegoSample != null)
                buttonLegoSample.onClick.RemoveListener(OnLegoSampleClicked);
        }
    }
}
