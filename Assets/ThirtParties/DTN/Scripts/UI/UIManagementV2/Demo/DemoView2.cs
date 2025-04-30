using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DemoView2 : DTNView
{
    public override void Init()
    {

    }

    public void Next()
    {
        ShowSubView<DemoView1>();
    }
}
