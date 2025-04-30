using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DTNUIManagerV2;

public class DemoView1 : DTNView
{
    public override void Init()
    {
        
    }

    public void Next()
    {
        ShowSubView<DTNPickerWheelPopUp>();
    }
}
