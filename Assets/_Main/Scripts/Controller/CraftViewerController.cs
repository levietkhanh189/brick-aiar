using UnityEngine;

public class CraftViewerController : MonoBehaviour
{
    private static CraftViewerController instance;

    public static CraftViewerController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<CraftViewerController>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("Craft Viewer Controller");
                    instance = obj.AddComponent<CraftViewerController>();
                }
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public string viewLegoPath = "Assets/_Main/LDraws/car.ldr";
}
