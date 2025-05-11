using UnityEngine;

public class ShowFirstViewController : MonoBehaviour
{
    public string ScreenName = "IntroScreen";

    void Start()
    {
        DTNWindow.FindTopWindow().ShowSubView(ScreenName);
    }
}
