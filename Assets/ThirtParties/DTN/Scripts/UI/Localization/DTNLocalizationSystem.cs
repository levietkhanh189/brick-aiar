using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DTNLocalizationSystem : MonoBehaviour
{
    [System.Serializable]
    public class DTNLanguageText
    {
        public string key;
        public string translated;
        public DTNLanguageText(string _key, string _tra)
        {
            key = _key;
            translated = _tra;
        }
    }

    [System.Serializable]
    public class DTNLocalLanguage
    {
        public List<DTNLanguageText> items;
    }
    private static DTNLocalLanguage localLanguage = null;
    private static Dictionary<string, string> localLanguageHasktable = new Dictionary<string, string>();

    public static DTNLocalizationSystem Instance;

    private void Awake()
    {
        Instance = this;
    }

    public string GetText(string key)
    {
        if (localLanguageHasktable.ContainsKey(key)){
            return localLanguageHasktable[key];
        }

        return key;
    }

    public void SetLanguage(string value)
    {
        TextAsset jsonFile = (TextAsset)Resources.Load("language/" + value + "", typeof(TextAsset));// duoi phai la duoi .json

        if (jsonFile != null)
        {
            string jsonString = jsonFile.text;// Resources.Load("language/" + Language() + ".json");
            localLanguage = JsonUtility.FromJson<DTNLocalLanguage>(jsonString);

            if (localLanguage != null)
            {
                localLanguageHasktable = new Dictionary<string, string>();
                foreach (DTNLanguageText ltext in localLanguage.items)
                {
                        localLanguageHasktable.Add(ltext.key, ltext.translated);
                }
            }

        }
    }
}
