using UnityEngine;

public class LoadingManager : MonoBehaviour
{
    public string ScreenName;

    void Start()
    {
        DTNWindow.FindTopWindow().ShowSubView(ScreenName);
    }
}
