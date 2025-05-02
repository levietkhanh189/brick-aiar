using System;
using System.Collections;
using UnityEngine;

namespace API
{
    public class APIManager : MonoBehaviour
    {
        private GenImageAPI genImageAPI;
        private GenLegoAPI genLegoAPI;

        private void Awake()
        {
            genImageAPI = new GenImageAPI();
            genLegoAPI = new GenLegoAPI();
        }

        public IEnumerator CallGenImage(string prompt, Action<GenImageResponseBody, string> callback)
        {
            yield return genImageAPI.GenImageCoroutine(prompt, callback);
        }

        public IEnumerator CallGenLego(string base64Image, Action<GenLegoResponseBody, string> callback)
        {
            yield return genLegoAPI.GenLegoCoroutine(base64Image, callback);
        }
    }
}
