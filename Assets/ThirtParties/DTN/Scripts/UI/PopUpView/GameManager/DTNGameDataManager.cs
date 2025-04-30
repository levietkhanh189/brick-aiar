using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using DTN.Iap;

public class DTNGameDataManager : MonoBehaviour
{
    public static DTNGameDataManager Instance;
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    private void Start()
    {
        Debug.Log("DTNGameDataManager Start");
       /* DTNInApp.Instance.BindingToInApp("com.fc.pl.be.FNF.full.mod.removeAds", () =>
        {
            Debug.Log("com.fc.pl.be.FNF.full.mod.removeAds start");
            RemoveAdsLiveData.Set(true, true);
        });

        DTNInApp.Instance.BindingToInApp("com.fc.pl.be.FNF.full.mod.vippass", () =>
        {
            Debug.Log("com.fc.pl.be.FNF.full.mod.vippass Start");
            RemoveAdsLiveData.Set(true, true);
            VipPassLiveData.Set(true, true);
        });
       */
    }


    public DTNLiveData<long> DiamondLiveData = new DTNLiveData<long>("DiamondLiveData",
        //Save
        (DTNLiveData<long> liveData) => {
            PlayerPrefs.SetString(liveData.name, liveData.Get() + "");
        },
        //Get
        (DTNLiveData<long> liveData) => {
            return long.Parse(PlayerPrefs.GetString(liveData.name,"10"));
        });

    public DTNLiveData<int> HeartLiveData = new DTNLiveData<int>("HeartLiveData",
        //Save
        (DTNLiveData<int> liveData) => {
            PlayerPrefs.SetInt(liveData.name, liveData.Get());
        },
        //Get
        (DTNLiveData<int> liveData) => {
            return PlayerPrefs.GetInt(liveData.name, 2);
        });

   /* public DTNLiveData<bool> RemoveAdsLiveData = new DTNLiveData<bool>("RemoveAdsLiveData",
        //Save
        (DTNLiveData<bool> liveData) => {
            PlayerPrefs.SetInt(liveData.name, liveData.Get()?1:0);
        },
        //Get
        (DTNLiveData<bool> liveData) => {
            if (IAPManager.Instance.IsPurchased("com.fc.pl.be.FNF.full.mod.removeAds") == true)
            {
                return true;
            }
            return PlayerPrefs.GetInt(liveData.name, 0)==1;
        });

    public DTNLiveData<bool> VipPassLiveData = new DTNLiveData<bool>("VipPassLiveData",
        //Save
        (DTNLiveData<bool> liveData) => {
            PlayerPrefs.SetInt(liveData.name, liveData.Get() ? 1 : 0);
        },
        //Get
        (DTNLiveData<bool> liveData) => {
            if (IAPManager.Instance.IsPurchased("com.fc.pl.be.FNF.full.mod.vippass") == true)
            {
                return true;
            }
            return PlayerPrefs.GetInt(liveData.name, 0) == 1;
        }); */

    public DTNLiveData<long> LuckyLiveData = new DTNLiveData<long>("LuckyLiveData",
        //Save
        (DTNLiveData<long> liveData) => {
            PlayerPrefs.SetString(liveData.name, liveData.Get() + "");
        },
        //Get
        (DTNLiveData<long> liveData) => {
            return long.Parse(PlayerPrefs.GetString(liveData.name, "10"));
        });

}
