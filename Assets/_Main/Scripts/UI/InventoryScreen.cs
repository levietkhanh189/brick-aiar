using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;

namespace UI
{
    public class InventoryScreen : DTNView
    {
        [SerializeField] private CanvasGroup canvasGroup;

        [SerializeField] private Button buttonClose;
        [SerializeField] private Button buttonAddLego;
        [SerializeField] private Transform buttonContent;
        [SerializeField] private LegoItem legoItemPrefab;
        [SerializeField] private List<LegoItem> legoItems;

        [SerializeField] private float fadeDuration = 0.5f;

        private MainSceneManager mainSceneManager;

        public override void Init()
        {
            SetupButtons();
            SetupMainSceneManager();
            SubscribeToEvents();
        }

        private void SetupMainSceneManager()
        {
            mainSceneManager = FindObjectOfType<MainSceneManager>();
            if (mainSceneManager == null)
            {
                Debug.LogError("Không tìm thấy MainSceneManager trong scene!");
            }
        }

        private void SubscribeToEvents()
        {
            MainSceneManager.OnModelReady += OnModelReady;
            MainSceneManager.OnLoadingProgress += OnLoadingProgress;
        }

        private void OnDestroy()
        {
            MainSceneManager.OnModelReady -= OnModelReady;
            MainSceneManager.OnLoadingProgress -= OnLoadingProgress;
        }

        private void SetupButtons()
        {
            if (buttonClose != null)
                buttonClose.onClick.AddListener(OnCloseClicked);
            if (buttonAddLego != null)
                buttonAddLego.onClick.AddListener(OnAddLegoClicked);
        }

        public override void Show()
        {
            base.Show();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
                canvasGroup.DOFade(1, fadeDuration);
            }
            
            LoadInventoryData();
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

        private void OnAddLegoClicked()
        {
            Debug.Log("Image To Lego button clicked");
            var popup_AICraft = DTNWindow.FindTopWindow().ShowSubView<Popup_AICraftOption>();
            popup_AICraft.InitIfNeed();
            popup_AICraft.Show();
            Hide();
        }

        #region Inventory Management

        private void LoadInventoryData()
        {
            if (mainSceneManager == null) return;

            ClearInventoryItems();

            var modelsWithLDR = mainSceneManager.GetModelsWithLDR();
            
            Debug.Log($"Đang load {modelsWithLDR.Count} models vào inventory");

            foreach (var modelId in modelsWithLDR)
            {
                var modelData = mainSceneManager.GetModel(modelId);
                var ldrPath = mainSceneManager.GetModelLDRPath(modelId);
                
                if (modelData != null && !string.IsNullOrEmpty(ldrPath))
                {
                    CreateLegoItem(modelData, ldrPath);
                }
            }
        }

        private void CreateLegoItem(API.LegoModelData modelData, string ldrPath)
        {
            if (legoItemPrefab == null || buttonContent == null)
            {
                Debug.LogError("LegoItem prefab hoặc buttonContent chưa được assign!");
                return;
            }

            GameObject itemObj = Instantiate(legoItemPrefab.gameObject, buttonContent);
            LegoItem legoItem = itemObj.GetComponent<LegoItem>();

            if (legoItem != null)
            {
                SetupLegoItem(legoItem, modelData, ldrPath);
                
                legoItems.Add(legoItem);
            }
        }

        /// <summary>
        /// Setup data cho LegoItem
        /// </summary>
        private void SetupLegoItem(LegoItem legoItem, API.LegoModelData modelData, string ldrPath)
        {
            // Sử dụng Initialize method mới của LegoItem
            legoItem.Initialize(modelData, ldrPath);

            // Set trạng thái new (có thể dựa vào thời gian tạo hoặc flag khác)
            legoItem.isNew = IsNewModel(modelData);
            
            // Cập nhật display
            legoItem.UpdateDisplay();

            // Setup button click event với callback
            legoItem.SetupButton(OnLegoItemClicked);

            Debug.Log($"Đã setup LegoItem: {modelData.name} - LDR: {ldrPath}");
        }

        private bool IsNewModel(API.LegoModelData modelData)
        {
            float currentTime = Time.time;
            float modelCreatedTime = modelData.created_at;
            float timeDifference = currentTime - modelCreatedTime;
            
            return timeDifference < 86400f;
        }

        /// <summary>
        /// Xử lý khi click vào LegoItem
        /// </summary>
        private void OnLegoItemClicked(LegoItem legoItem)
        {
            if (legoItem == null || !legoItem.IsValid())
            {
                Debug.LogWarning("LegoItem không hợp lệ!");
                return;
            }

            var modelData = legoItem.GetModelData();
            Debug.Log($"Clicked on Lego model: {modelData.name}");
            Debug.Log($"LDR Path: {legoItem.localLDRPath}");
            
            // Đánh dấu là đã xem
            legoItem.MarkAsViewed();

            LoadLegoModel(legoItem.localLDRPath, modelData);
        }

        private void LoadLegoModel(string ldrPath, API.LegoModelData modelData)
        {
            Debug.Log($"Đang load model LEGO: {modelData.name} từ path: {ldrPath}");
            CraftViewerController.Instance.viewLegoPath = ldrPath;
            GameSceneManager.Instance.LoadScene("CraftViewer3D");
        }

        private void ClearInventoryItems()
        {
            foreach (var item in legoItems)
            {
                if (item != null && item.gameObject != null)
                {
                    DestroyImmediate(item.gameObject);
                }
            }
            legoItems.Clear();
        }

        #endregion

        #region Event Handlers

        private void OnModelReady(API.LegoModelData modelData, string ldrFilePath)
        {
            Debug.Log($"Model mới sẵn sàng: {modelData.name}");
            
            if (gameObject.activeInHierarchy)
            {
                CreateLegoItem(modelData, ldrFilePath);
            }
        }

        private void OnLoadingProgress(int loaded, int total)
        {
            Debug.Log($"Loading progress: {loaded}/{total}");
            // TODO: Có thể hiển thị progress bar nếu cần
        }

        #endregion
    }
}
