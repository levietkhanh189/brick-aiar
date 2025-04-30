using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DTNDailyRewardManager : DTNMono
{
    public static DTNDailyRewardManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
            
    }

    private string TodayString()
    {
        System.DateTime dt = System.DateTime.Now;
        //return dt.ToString("yyyy-MM-dd");
        return dt.ToString("yyyy-mm-dd HH:mm");
    }

    private string OldDayOpenGame()
    {
        return PlayerPrefs.GetString("OldDayOpenGame", "");
    }

    private void SaveDayOpenGame(string dayStr)
    {
        PlayerPrefs.SetString("OldDayOpenGame", dayStr);
    }

    public int OpenGameCount()
    {
        return PlayerPrefs.GetInt("OldDayOpenGameCount", -1);
    }

    public void SaveDayOpenGameCount(int count)
    {
        PlayerPrefs.SetInt("OldDayOpenGameCount", count);
    }

    public int OpenGameCountDailyClaimed()
    {
        return PlayerPrefs.GetInt("OldDayOpenGameCountDailyClaimed", -1);
    }

    public void SaveDayOpenGameCountDailyClaimed(int count)
    {
        PlayerPrefs.SetInt("OldDayOpenGameCountDailyClaimed", count);
    }

    private void OnApplicationFocus(bool focus)
    {
        if (OldDayOpenGame() != TodayString())
        {
            SaveDayOpenGame(TodayString());
            SaveDayOpenGameCount(OpenGameCount() + 1);
        }
    }

    public void ResetData()
    {
        PlayerPrefs.DeleteAll();
    }
}
