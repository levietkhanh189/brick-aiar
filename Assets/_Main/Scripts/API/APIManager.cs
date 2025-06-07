using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace API
{
    public class APIManager : MonoBehaviour
    {
        public static APIManager Instance { get; private set; }
        
        private GenImageAPI genImageAPI;
        private GenLegoAPI genLegoAPI;
        
        // Callback dictionary để lưu trữ callbacks cho các request async
        private Dictionary<string, Action<LegoModelData, string>> legoCallbacks = new Dictionary<string, Action<LegoModelData, string>>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAPIs();
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void InitializeAPIs()
        {
            genImageAPI = new GenImageAPI();
            genLegoAPI = new GenLegoAPI();
        }

        /// <summary>
        /// Gọi API để tạo ảnh - trả kết quả ngay lập tức
        /// </summary>
        public IEnumerator CallGenImage(string prompt, Action<GenImageResponseBody, string> callback)
        {
            yield return genImageAPI.GenImageCoroutine(prompt, callback);
        }

        /// <summary>
        /// Gọi API để tạo LEGO - trả về requestId và listen Firebase để nhận kết quả
        /// </summary>
        public IEnumerator CallGenLego(string base64Image, Action<LegoModelData, string> onComplete, float details = 0.02f, float foregroundRatio = 0.85f)
        {
            yield return StartCoroutine(genLegoAPI.GenLegoCoroutine(base64Image, (response, error) =>
            {
                if (error != null)
                {
                    onComplete?.Invoke(null, error);
                    return;
                }

                if (response != null && !string.IsNullOrEmpty(response.requestId))
                {
                    Debug.Log($"Đã gửi request LEGO thành công. Request ID: {response.requestId}");
                    Debug.Log("Đang chờ xử lý từ AWS...");
                }
                else
                {
                    onComplete?.Invoke(null, "Không nhận được requestId từ server");
                }
            }, details, foregroundRatio));
        }

        /// <summary>
        /// Hủy listen cho một request cụ thể
        /// </summary>
        public void CancelLegoRequest(string requestId)
        {
            if (legoCallbacks.ContainsKey(requestId))
            {
                legoCallbacks.Remove(requestId);
            }
        }

        private void OnDestroy()
        {
            legoCallbacks.Clear();
        }
    }
}
