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
        private RealtimeDatabaseListener realtimeListener;
        
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
            
            // Khởi tạo RealtimeDatabaseListener
            realtimeListener = gameObject.AddComponent<RealtimeDatabaseListener>();
            realtimeListener.OnModelCompleted += OnLegoModelCompleted;
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
                    // Lưu callback để gọi khi có kết quả từ Firebase
                    legoCallbacks[response.requestId] = onComplete;
                    
                    // Bắt đầu listen cho request này
                    realtimeListener.StartListeningForRequest(response.requestId);
                    
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
        /// Xử lý khi nhận được kết quả LEGO từ Firebase
        /// </summary>
        private void OnLegoModelCompleted(string requestId, LegoModelData modelData, string error)
        {
            if (legoCallbacks.TryGetValue(requestId, out Action<LegoModelData, string> callback))
            {
                callback?.Invoke(modelData, error);
                legoCallbacks.Remove(requestId); // Cleanup
            }
        }

        /// <summary>
        /// Hủy listen cho một request cụ thể
        /// </summary>
        public void CancelLegoRequest(string requestId)
        {
            if (legoCallbacks.ContainsKey(requestId))
            {
                legoCallbacks.Remove(requestId);
                realtimeListener.StopListeningForRequest(requestId);
            }
        }

        private void OnDestroy()
        {
            if (realtimeListener != null)
            {
                realtimeListener.OnModelCompleted -= OnLegoModelCompleted;
            }
            legoCallbacks.Clear();
        }
    }
}
