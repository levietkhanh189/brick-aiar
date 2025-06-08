using UnityEngine;

public class ViewerManager : MonoBehaviour
{
    public LDrawImporter drawImporter;

    public void Start()
    {
        drawImporter.ImportLDrawModel(CraftViewerController.Instance.viewLegoPath);
    }

    public void GoToARMode()
    {
        GameSceneManager.Instance.LoadScene("CraftViewerAR");
    }

    public void GoTo3DMode()
    {
        GameSceneManager.Instance.LoadScene("CraftViewer3D");
    }

    public void GoToHome()
    {
        GameSceneManager.Instance.LoadScene("MainScene");
    }
}
