using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance { get; private set; }

    [Header("Scene Settings")]
    [SerializeField] private float minimumLoadingTime = 1f;

    private string currentSceneName;
    private bool isLoading = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        currentSceneName = SceneManager.GetActiveScene().name;
    }

    public void LoadScene(string sceneName, Action onComplete = null)
    {
        if (isLoading) return;

        StartCoroutine(LoadSceneAsync(sceneName, onComplete));
    }

    private IEnumerator LoadSceneAsync(string sceneName, Action onComplete)
    {
        isLoading = true;

        // Load target scene
        AsyncOperation sceneOperation = SceneManager.LoadSceneAsync(sceneName);
        sceneOperation.allowSceneActivation = false;

        float loadingTimer = 0f;
        while (loadingTimer < minimumLoadingTime || sceneOperation.progress < 0.9f)
        {
            loadingTimer += Time.deltaTime;
            yield return null;
        }

        sceneOperation.allowSceneActivation = true;
        while (!sceneOperation.isDone)
        {
            yield return null;
        }

        currentSceneName = sceneName;
        isLoading = false;
        onComplete?.Invoke();
    }

    public void ReloadCurrentScene()
    {
        LoadScene(currentSceneName);
    }

    public string GetCurrentSceneName()
    {
        return currentSceneName;
    }

    public bool IsLoading()
    {
        return isLoading;
    }
}
