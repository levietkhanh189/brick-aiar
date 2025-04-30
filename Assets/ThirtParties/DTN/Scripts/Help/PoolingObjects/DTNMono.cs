using UnityEngine;
using System.Collections;

public class DTNMono : MonoBehaviour
{
    public System.Action destroyEvent;

    public IEnumerator EnumDoItAfter(float after, System.Action action)
    {
        yield return new WaitForSeconds(after);
        action();
    }

    public void DoItAfter(float after, System.Action action)
    {
        StartCoroutine(EnumDoItAfter(after, action));
    }

    public virtual void OnDestroy()
    {
        destroyEvent?.Invoke();
    }
}
