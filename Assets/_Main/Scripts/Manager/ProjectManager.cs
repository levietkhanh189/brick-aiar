using UnityEngine;

public class ProjectManager : MonoBehaviour
{
    public void GoToHome()
    {
        GameSceneManager.Instance.LoadScene("MainScene");
    }
}
