using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DTNPoolingGameItem : DTNMono
{
    public int key;
    public bool isUsing;
    public bool isWaitingForDestroy = false;

    public void DestroyAllChildIfNeed()
    {
        DTNPoolingGameItem[] list = GetComponentsInChildren<DTNPoolingGameItem>();

        foreach (DTNPoolingGameItem item in list)
        {
            item.DestroyIfNeed();
        }
    }

    private void DestroyIfNeed()
    {
        if (isUsing == true && isWaitingForDestroy)
        {
            DTNPoolingGameManager.Instance.DestroyObject(this, false);
        }
    }

    public void DestroyAfter(float after)
    {
        StopAllCoroutines();
        isWaitingForDestroy = true;
        DoItAfter(after, () =>
        {
            DTNPoolingGameManager.Instance.DestroyObject(this);
        });
    }

    //public override void OnDestroy()
    //{
    //    DTNPoolingGameManager.Instance.DestroyObject(this);
    //    base.OnDestroy();
    //}
}
