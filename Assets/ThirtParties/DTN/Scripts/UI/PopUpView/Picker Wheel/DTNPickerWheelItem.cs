using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DTNPickerWheelItem : MonoBehaviour
{
    private string ItemName;
    private int Amount;
    public Image ItemImage;
    public Text AmountText;

    public void SetItem(string name,Sprite sprite,int amount)
    {
        ItemName = name;
        Amount = amount;
        ItemImage.sprite = sprite;
        AmountText.text = amount + "";
    }

    public void GetReward()
    {
        int current = PlayerPrefs.GetInt(ItemName);
        PlayerPrefs.SetInt(ItemName, Amount + current);
    }
}
