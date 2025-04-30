using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
[CreateAssetMenuAttribute(fileName = "DailyRewardsDatabase", menuName = "Data/Scriptable/DailyRewardsDatabase")]
public class DTNDailyRewardDataSystem : ScriptableObject
{
    public List<DTNDailyItem.DailyRewardItemData> DailyRewardsDatas;
    public int dayTime = 86400;
    private Hashtable hashtable = new Hashtable();

    private void GenerateHashtable()
    {
        for (int i = 0; i < DailyRewardsDatas.Count; i++)
        {
            hashtable.Add(DailyRewardsDatas[i].Day, DailyRewardsDatas[i]);
        }
    }

    public DTNDailyItem.DailyRewardItemData GetRewardItem(int day)
    {
        if (hashtable.Count <= 0)
            GenerateHashtable();
        return DailyRewardsDatas[day];
    }

    public int DayReward()
    {
        int day = PlayerPrefs.GetInt("DailyReward", 1);
        if ((CheckReward() || day == 1) && day <= DailyRewardsDatas.Count)
        {
            SetRewardClampDateTime();
            PlayerPrefs.SetInt("DailyReward", day + 1);
            return day;
        }

        return -1;
    }

    public bool CheckReward()
    {
        DateTime currentDateTime = DateTime.Now;
        DateTime rewardClampDateTime = DateTime.Parse(PlayerPrefs.GetString("Reward_Clamp_DateTime", currentDateTime.ToString()));

        if ((currentDateTime - rewardClampDateTime).TotalSeconds >= dayTime)
            return true;

        return false;
    }

    private void SetRewardClampDateTime()
    {
        PlayerPrefs.SetString("Reward_Clamp_DateTime", DateTime.Now.ToString());
    }

    private void OnDestroy()
    {
        SetRewardClampDateTime();
    }
}
