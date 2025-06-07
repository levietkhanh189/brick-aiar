using System;
using System.Collections;
using API;
using UnityEngine;

public class AIFlowController : MonoBehaviour
{
    public static AIFlowController Instance;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void CraftTextToLego(string promt, float details = 0.02f, float foregroundRatio = 0.85f)
    {
        GenerateImage(promt, (string imageBase64) =>
        {
            GenerateLego(imageBase64, ListenToFirebaseRealtime, details, foregroundRatio);
        });
    }

    public void CraftImageToLego(string imageBase64, float details = 0.02f, float foregroundRatio = 0.85f)
    {
        GenerateLego(imageBase64, ListenToFirebaseRealtime, details, foregroundRatio);
    }

    public void ListenToFirebaseRealtime(LegoModelData modelData, string id)
    {

    }

    private void GenerateLego(string base64Image, Action<LegoModelData, string> onComplete, float details = 0.02f, float foregroundRatio = 0.85f)
    {
        StartCoroutine(GenerateLegoFlow(base64Image, onComplete, details, foregroundRatio));
    }

    private IEnumerator GenerateLegoFlow(string base64Image, Action<LegoModelData, string> onComplete, float details = 0.02f, float foregroundRatio = 0.85f)
    {
        yield return APIManager.Instance.CallGenLego(base64Image, onComplete, details, foregroundRatio);
    }

    private void GenerateImage(string promt, Action<string> onComplete)
    {
        StartCoroutine(GenerateImageFlow(promt, onComplete));
    }

    private IEnumerator GenerateImageFlow(string promt,Action<string> onComplete)
    {
        yield return APIManager.Instance.CallGenImage(promt, (response, error) =>
        {
            if (response != null && !string.IsNullOrEmpty(response.image))
                onComplete?.Invoke(response.image);
        });
    }
}
