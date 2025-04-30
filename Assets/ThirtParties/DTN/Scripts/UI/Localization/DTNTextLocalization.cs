using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DTNTextLocalization : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        Text text = GetComponent<Text>();
        if (text != null)
        {
            text.text = DTNLocalizationSystem.Instance.GetText(text.text);
        }
    }
}
