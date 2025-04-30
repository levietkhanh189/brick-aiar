using UnityEngine;
using System.Collections;

using UnityEngine.Networking;

public class DTNLoadNetwork : MonoBehaviour
{
    private static DTNLoadNetwork instance;
    public static DTNLoadNetwork Instance
    {
        get
        {
            if (instance == null) Init();

            return instance;
        }
    }

    public bool IsInitialized { get; set; }

    public static void Init()
    {
        if (instance != null) return;

        if (Application.isPlaying)
        {
            var obj = new GameObject("DTNLoadNetwork") { hideFlags = HideFlags.HideAndDontSave };
            instance = obj.AddComponent<DTNLoadNetwork>();
            DontDestroyOnLoad(obj);
        }
    }

    public Coroutine StartDownloadAudioClip(string url, AudioType type, System.Action<AudioClip, string> complete)
    {
        return StartCoroutine(GetAudioClip(url, type, complete));
    }

    public Coroutine StartDownloadText(string url, System.Action<string, string> complete)
    {
        return StartCoroutine(GetTextContent(url, complete));
    }

    IEnumerator GetTextContent(string url, System.Action<string, string> complete)
    {
#if UNITY_EDITOR
        yield return new WaitForSeconds(3);
#endif
        Debug.Log("Downloading: " + url);
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                complete(null, www.error);
            }
            else
            {
                try
                {
                    string stringData = www.downloadHandler.text;
                    complete(stringData, null);
                }
                catch (System.Exception e)
                {
                    complete(null, e.Message);
                }

            }
        }
    }




    IEnumerator GetAudioClip(string url, AudioType type, System.Action<AudioClip, string> complete)
    {
#if UNITY_EDITOR
        yield return new WaitForSeconds(3);
#endif
        Debug.Log("Downloading: " + url);
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, type))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                complete(null, www.error);
            }
            else
            {
                try
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    complete(clip, null);
                }
                catch (System.Exception e)
                {
                    complete(null, e.Message);
                }
                
            }
        }
    }
}
