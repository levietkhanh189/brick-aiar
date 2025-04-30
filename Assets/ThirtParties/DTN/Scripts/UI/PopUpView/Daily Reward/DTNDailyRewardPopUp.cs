using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace DTNUIManagerV2
{
    public class DTNDailyRewardPopUp : DTNView
    {
        public List<DTNDailyItem> DailyItems = new List<DTNDailyItem>();
        public DTNDailyRewardDataSystem DailyRewardDataSystem;
        public GameObject templateCell;
       
        private void Awake()
        {
            Show();
        }

        public override void Init()
        {
            
            foreach (DTNDailyItem.DailyRewardItemData data in DailyRewardDataSystem.DailyRewardsDatas)
            {
                GameObject nGameObject = Instantiate(templateCell, templateCell.transform.parent);
                DTNDailyItem item = nGameObject.GetComponent<DTNDailyItem>();
                DailyItems.Add(item);
                item.SetUp(data,(DTNDailyItem dailyItem) => {
                    DTNGameDataManager.Instance.DiamondLiveData.Set(DTNGameDataManager.Instance.DiamondLiveData.Get() + data.Amount);
                    DTNGameDataManager.Instance.DiamondLiveData.Notify();
                    DTNDailyRewardManager.Instance.SaveDayOpenGameCountDailyClaimed(DTNDailyRewardManager.Instance.OpenGameCountDailyClaimed() + 1);
                    UpdateUI();
                });
                item.gameObject.SetActive(true);
            }
        }

        public void UpdateUI()
        {
            foreach (DTNDailyItem item in DailyItems)
            {
                item.UpdateUI();
            }
        }

        public override void Show()
        {
            UpdateUI();
            base.Show();
        }

        public override void Hide()
        {
            base.Hide();
        }
    }
}