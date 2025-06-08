using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;

namespace UI
{
    public class MainScreen : DTNView
    {
        [SerializeField] private CanvasGroup canvasGroup;

        [SerializeField] private Image userPicture;
        [SerializeField] private TextMeshProUGUI userNameText;

        [SerializeField] private TextMeshProUGUI statusCoinText;
        [SerializeField] private Button statusCoinAddButton;

        [SerializeField] private TextMeshProUGUI statusGemText;
        [SerializeField] private Button statusGemAddButton;

        [SerializeField] private Button buttonSetting;
        [SerializeField] private Button buttonInventory;
        [SerializeField] private Button buttonShop;
        [SerializeField] private Button buttonFriends;
        [SerializeField] private Button buttonClan;
        [SerializeField] private Button buttonCraft;
        [SerializeField] private Button buttonProjects;

        [SerializeField] private float fadeDuration = 0.5f;

        private DataController dataController;
        private UserInfo userInfo;

        public override void Init()
        {
            dataController = FindObjectOfType<DataController>();
            userInfo = UserInfo.Instance;

            SetupButtons();
            UpdateUI();
        }

        private void SetupButtons()
        {
            if (statusCoinAddButton != null)
                statusCoinAddButton.onClick.AddListener(OnStatusCoinAddClicked);
            if (statusGemAddButton != null)
                statusGemAddButton.onClick.AddListener(OnStatusGemAddClicked);

            if (buttonSetting != null)
                buttonSetting.onClick.AddListener(OnButtonSettingClicked);
            if (buttonInventory != null)
                buttonInventory.onClick.AddListener(OnButtonInventoryClicked);
            if (buttonShop != null)
                buttonShop.onClick.AddListener(OnButtonShopClicked);
            if (buttonFriends != null)
                buttonFriends.onClick.AddListener(OnButtonFriendsClicked);
            if (buttonClan != null)
                buttonClan.onClick.AddListener(OnButtonClanClicked);
            if (buttonCraft != null)
                buttonCraft.onClick.AddListener(OnButtonCraftClicked);
            if (buttonProjects != null)
                buttonProjects.onClick.AddListener(OnButtonProjectsClicked);
        }

        public override void Show()
        {
            base.Show();
            canvasGroup.alpha = 0;
            canvasGroup.DOFade(1, fadeDuration);
            UpdateUI();

        }

        public override void Hide()
        {
            canvasGroup.DOFade(0, fadeDuration)
                .OnComplete(() => base.Hide());
        }

        private void UpdateUI()
        {
            if (userInfo == null) return;

            if (userNameText != null)
                userNameText.text = userInfo.UserName;

        }

        private void OnStatusLifeAddClicked()
        {
            Debug.Log("Status Life Add button clicked");
            // Implement logic to add life
        }

        private void OnStatusCoinAddClicked()
        {
            Debug.Log("Status Coin Add button clicked");
            // Implement logic to add coin
        }

        private void OnStatusGemAddClicked()
        {
            Debug.Log("Status Gem Add button clicked");
            // Implement logic to add gem
        }

        private void OnButtonSettingClicked()
        {
            Debug.Log("Settings button clicked");
            // Implement logic to open settings popup or screen
        }

        private void OnButtonInventoryClicked()
        {
            Debug.Log("Inventory button clicked");
            Debug.Log("Craft button clicked");
            var inventoryScreen = DTNWindow.FindTopWindow().ShowSubView<InventoryScreen>();
            inventoryScreen.InitIfNeed();
            inventoryScreen.Show();
        }

        private void OnButtonShopClicked()
        {
            Debug.Log("Shop button clicked");
            // Implement logic to open shop screen
        }

        private void OnButtonFriendsClicked()
        {
            Debug.Log("Friends button clicked");
            // Implement logic to open friends screen
        }

        private void OnButtonClanClicked()
        {
            Debug.Log("Clan button clicked");
            // Implement logic to open clan screen
        }

        private void OnButtonCraftClicked()
        {
            Debug.Log("Craft button clicked");
            var popup_AICraft = DTNWindow.FindTopWindow().ShowSubView<Popup_AICraftOption>();
            popup_AICraft.InitIfNeed();
            popup_AICraft.Show();
        }

        private void OnButtonProjectsClicked()
        {
            Debug.Log("Projects button clicked");
            var projectScreen = DTNWindow.FindTopWindow().ShowSubView<ProjectScreen>();
            projectScreen.InitIfNeed();
            projectScreen.Show();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (statusCoinAddButton != null)
                statusCoinAddButton.onClick.RemoveListener(OnStatusCoinAddClicked);
            if (statusGemAddButton != null)
                statusGemAddButton.onClick.RemoveListener(OnStatusGemAddClicked);

            if (buttonSetting != null)
                buttonSetting.onClick.RemoveListener(OnButtonSettingClicked);
            if (buttonInventory != null)
                buttonInventory.onClick.RemoveListener(OnButtonInventoryClicked);
            if (buttonShop != null)
                buttonShop.onClick.RemoveListener(OnButtonShopClicked);
            if (buttonFriends != null)
                buttonFriends.onClick.RemoveListener(OnButtonFriendsClicked);
            if (buttonClan != null)
                buttonClan.onClick.RemoveListener(OnButtonClanClicked);
            if (buttonCraft != null)
                buttonCraft.onClick.RemoveListener(OnButtonCraftClicked);
            if (buttonProjects != null)
                buttonProjects.onClick.RemoveListener(OnButtonProjectsClicked);
        }
    }
}
