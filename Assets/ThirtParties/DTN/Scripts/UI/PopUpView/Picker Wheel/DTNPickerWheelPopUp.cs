using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DTNUIManagerV2
{
    [System.Serializable]
    public class DTNPickerWheelItemInfp
    {
        public string NameItem;
        public Sprite Icon;
        public int Amount;

        string Title
        {
            get
            {
                return NameItem;
            }
        }

        
    }

    public class DTNPickerWheelPopUp : DTNView
    {
        [Header("Item")]
        public DTNPickerWheelItemInfp[] PickerWheelItemInfos;
        [SerializeField]
        private DTNPickerWheelItem PickerWheelItemSample;
        [SerializeField]
        private List<DTNPickerWheelItem> PickerWheelItemList = new List<DTNPickerWheelItem>();

        [Header("Line")]
        [SerializeField]
        private GameObject LineSample;
        [SerializeField]
        private List<GameObject> LineList = new List<GameObject>();

        [Header("Rotation")]
        public Transform PickerWheelTrans;
        public float RotateTime;

        [SerializeField]
        private Text winText;


        [Header("ButtonSpin")]

        [SerializeField]
        private GameObject buttonFreeSpin;
        [SerializeField]
        private Text freeSpinText;

        [SerializeField]
        private GameObject buttonAdsSpin;


        public override void Init()
        {

        }

        private void UpdateButtonStatus()
        {

            DTNLuckyWheelManager.SpinStatus status = DTNLuckyWheelManager.Instance.SpiningStatus.Get();
            if (status == DTNLuckyWheelManager.SpinStatus.DONE || status == DTNLuckyWheelManager.SpinStatus.IDLE)
            {

                buttonFreeSpin.active = true;
                buttonAdsSpin.active = DTNLuckyWheelManager.Instance.countDown.Get() > 0;
                
            }
            else
            {
                buttonFreeSpin.active = false;
                buttonAdsSpin.active = false;
            }
        }

        public void Start()
        {
            System.Action<DTNLuckyWheelManager.SpinStatus> binding1 = DTNLuckyWheelManager.Instance.SpiningStatus.Binding((DTNLuckyWheelManager.SpinStatus status) => {
                UpdateButtonStatus();
            });


            System.Action<long> binding2 = DTNLuckyWheelManager.Instance.countDown.Binding((long timeInterval) => {
                UpdateButtonStatus();
                if (timeInterval>0)
                {
                    freeSpinText.text = DTNDate.GetDayHouseMinSecondString(timeInterval);
                    //freeSpinText.text = timeInterval + "";
                }
                else
                {
                    freeSpinText.text = "Spin";
                }
            });

            this.destroyEvent += () =>
            {
                DTNLuckyWheelManager.Instance.SpiningStatus.UnBinding(binding1);
                DTNLuckyWheelManager.Instance.countDown.UnBinding(binding2);
            };
        }

        public override void Show()
        {
            base.Show(); 
        }

        public override void Hide()
        {
            base.Hide();
        }

        public void GenerateWheel()
        {
            GenerateLineList();
            GenerateItems();
        }

        public void GenerateWheel2()
        {
            GenerateLineList();
            GenerateItems();
        }

        public void Spin()
        {
            StartCoroutine(IEnumSpin());
        }

        private void GenerateLineList()
        {
            LineSample.SetActive(true);

            foreach (var item in LineList)
            {
                DestroyImmediate(item);
            }

            LineList = new List<GameObject>();

            for (int i = 0; i < PickerWheelItemInfos.Length; i++)
            {
                GameObject line = Instantiate(LineSample, transform.GetChild(0));
                line.transform.localEulerAngles = new Vector3(0, 0, i * ((360 / PickerWheelItemInfos.Length)));
                LineList.Add(line);
            }

            LineSample.SetActive(false);
        }

        private void GenerateItems()
        {
            PickerWheelItemSample.gameObject.SetActive(true);

            if(PickerWheelItemList.Count > 0)
                foreach (var item in PickerWheelItemList)
                    DestroyImmediate(item.gameObject);

            PickerWheelItemList = new List<DTNPickerWheelItem>();

            for (int i = 0; i < PickerWheelItemInfos.Length; i++)
            {
                DTNPickerWheelItem item = Instantiate(PickerWheelItemSample.gameObject, transform.GetChild(0)).GetComponent<DTNPickerWheelItem>();
                item.transform.localEulerAngles = new Vector3(0, 0, -(i+0.5f) * ((360 / PickerWheelItemInfos.Length)));
                Debug.Log("Item " + i + " -- " + +(i + 0.5f) * ((360 / PickerWheelItemInfos.Length)));
                item.SetItem(PickerWheelItemInfos[i].NameItem, PickerWheelItemInfos[i].Icon, PickerWheelItemInfos[i].Amount);

                PickerWheelItemList.Add(item);
            }

            PickerWheelItemSample.gameObject.SetActive(false);
        }

        private IEnumerator IEnumSpin()
        {

            DTNLuckyWheelManager.Instance.SpiningStatus.Set(DTNLuckyWheelManager.SpinStatus.SPINGING, true);
            int value = (int)(RotateTime/Time.deltaTime + Random.Range(RotateTime/4, RotateTime/2));
            float speed = value;
            for (int i = 0; i < value; i++)
            {
               // PickerWheelTrans.transform.Rotate(0, 0, speed * Time.deltaTime);
                PickerWheelTrans.transform.localEulerAngles += new Vector3(0, 0, speed * Time.deltaTime);
                speed--;
                yield return Time.deltaTime;
            }

            int finalAngle = (int)PickerWheelTrans.localEulerAngles.z;
            int degree = 360 / PickerWheelItemInfos.Length;
            int result = 0;

            for (int i = 0; i < PickerWheelItemInfos.Length; i++)
            {
                if (finalAngle >= i * degree && finalAngle < (i + 1) * degree)
                    result = i; 
            }

            DTNLuckyWheelManager.Instance.SpiningStatus.Set(DTNLuckyWheelManager.SpinStatus.DONE, true);

            Debug.Log(result);
            PickerWheelItemList[result].GetReward();
            winText.text = PickerWheelItemInfos[result].NameItem;
            DTNGameDataManager.Instance.DiamondLiveData.Set(DTNGameDataManager.Instance.DiamondLiveData.Get()+ PickerWheelItemInfos[result].Amount, true);
        }
    }
}
