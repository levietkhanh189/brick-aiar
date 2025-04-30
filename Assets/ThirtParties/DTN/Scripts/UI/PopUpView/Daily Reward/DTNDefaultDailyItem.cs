using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DTNDefaultDailyItem : DTNDailyItem
{
    public List<Text> titleTexts;
    public List<Text> itemTexts;
    public List<Image> icons;
    

    override
    public void SetUp(DTNDailyItem.DailyRewardItemData data, System.Action<DTNDailyItem> callback)
    {
        claimedCallback = callback;
        Day = data.Day;
        foreach(Text text in titleTexts)
        {
            text.text = "DAY " + Day;
        }

        foreach (Text text in itemTexts)
        {
            text.text = data.RewardName;
        }

        foreach (Image image in icons)
        {
            image.sprite = data.Icon;
        }
    }

    public void ClaimedButtonClick()
    {
        claimedCallback?.Invoke(this);
    }
}
