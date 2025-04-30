using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DTNDailyItem : MonoBehaviour
{
    [System.Serializable]
    public class DailyRewardItemData
    {
        [SerializeField]
        public int Day;
        [SerializeField]
        public string RewardName;
        [SerializeField]
        public Sprite Icon;
        [SerializeField]
        public int Amount;
    }

    public int Day;
    public GameObject inActiveDaily;
    public GameObject activeDaily;
    public GameObject receviedDaily;

    public System.Action<DTNDailyItem> claimedCallback;

    public virtual void OnEnable()
    {
        UpdateUI();
    }

    public virtual void UpdateUI()
    {
        if (ClaimedDay() >= Day)
        {
            inActiveDaily.gameObject.SetActive(false);
            activeDaily.gameObject.SetActive(false);
            receviedDaily.gameObject.SetActive(true);
        }
        else if (ClaimedDay() == Day - 1 && OpenGameCount() >= Day)
        {
            inActiveDaily.gameObject.SetActive(false);
            activeDaily.gameObject.SetActive(true);
            receviedDaily.gameObject.SetActive(false);
        }
        else
        {
            inActiveDaily.gameObject.SetActive(true);
            activeDaily.gameObject.SetActive(false);
            receviedDaily.gameObject.SetActive(false);
        }
    }

    public virtual void SetUp(DTNDailyItem.DailyRewardItemData data, System.Action<DTNDailyItem> callback)
    {
        
    }

    private int OpenGameCount()
    {
        return DTNDailyRewardManager.Instance.OpenGameCount();
    }

    private int ClaimedDay()
    {
        return DTNDailyRewardManager.Instance.OpenGameCountDailyClaimed();
    }
}
