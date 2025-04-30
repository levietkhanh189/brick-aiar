using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DTNLuckyWheelManager : DTNMono
{

    public enum SpinStatus
    {
        IDLE,
        SPINGING,
        DONE
    }
    public static DTNLuckyWheelManager Instance;
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

    public long timeIntervalCountDown = 60*5;//60 second

    public DTNLiveData<long> countDown = new DTNLiveData<long>("CountDown",null,null);
    public DTNLiveData<SpinStatus> SpiningStatus = new DTNLiveData<SpinStatus>("SpiningStatus", null, (DTNLiveData<SpinStatus> liveData) => {
        return SpinStatus.IDLE;
    });

    //public DTNLiveData<bool> isCountdown = new DTNLiveData<bool>("IsCountDown",
    //    //Save
    //    (DTNLiveData<bool> liveData) => {
    //        PlayerPrefs.SetInt(liveData.name, liveData.Get() == false?0:1);
    //    },
    //    //Get
    //    (DTNLiveData<bool> liveData) => {
    //        return (PlayerPrefs.GetInt(liveData.name, 0)==1);
    //    });

    public DTNLiveData<long> TimeUseLuckyWheel = new DTNLiveData<long>("TimeUseLuckyWheel",
        //Save
        (DTNLiveData<long> liveData) => {
            PlayerPrefs.SetString(liveData.name, liveData.Get() + "");
        },
        //Get
        (DTNLiveData<long> liveData) => {
            return long.Parse(PlayerPrefs.GetString(liveData.name, "86400"));
        });

    private Coroutine countDownEnumator = null;

    private void Start()
    {
        RegisterLiveData();
    }

    public void RegisterLiveData()
    {
        System.Action<long> TimeUseLuckyWheelBinding = TimeUseLuckyWheel.Binding((long timeInterval) => {
            CheckAndCountDown();
        });

        System.Action<SpinStatus> SpiningStatusBinding = SpiningStatus.Binding((SpinStatus status) => {
            if (status == SpinStatus.DONE)
            {
                TimeUseLuckyWheel.Set(DTNDate.NowTimeInterval(), true);
            }
        });

        this.destroyEvent += () =>
        {
            TimeUseLuckyWheel.UnBinding(TimeUseLuckyWheelBinding);
            SpiningStatus.UnBinding(SpiningStatusBinding);
        };
    }

    private void CheckAndCountDown()
    {
        if (timeIntervalCountDown - (DTNDate.NowTimeInterval() - TimeUseLuckyWheel.Get()) >= 0 )
        {
            countDown.Set(timeIntervalCountDown - (DTNDate.NowTimeInterval() - TimeUseLuckyWheel.Get()), true);
            if (countDownEnumator != null) StopCoroutine(countDownEnumator);
            countDownEnumator = StartCoroutine(CountDownEnumator());
        }
    }

    IEnumerator CountDownEnumator()
    {
        while (countDown.Get() > 0)
        {
            countDown.Set(countDown.Get()-1, true);
            yield return new WaitForSeconds(1);
        }

        countDown.Set(0, true);
        //isCountdown.Set(false, true);
    }

}
