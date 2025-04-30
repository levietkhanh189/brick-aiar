using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DTNWindow : DTNView
{
    public static List<DTNWindow> windows = new List<DTNWindow>();

    public int level = 0;
    private void Awake()
    {
        windows.Add(this);
    }

    public static DTNWindow FindTopWindow()
    {
        DTNWindow bestWindow = null;
        foreach (DTNWindow window in windows)
        {
            if (bestWindow == null || window.level > bestWindow.level)
            {
                bestWindow = window;
            }

        }
        return bestWindow;
    }

    public override void OnDestroy()
    {
        windows.Remove(this);
        base.OnDestroy();
    }

    public override void Init()
    {

    }
}