using UnityEngine;
namespace Battlehub.RTHandles.Demo
{
    public class CameraFlow : MonoBehaviour
    {
        public static CameraFlow Instance { get; private set; }

        [SerializeField] private Transform target;
        [SerializeField] private float smoothSpeed = 5f;
        [SerializeField] private Vector3 offset = new Vector3(0, 5, -10);

        private bool isFlowing = false;

        private void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void LateUpdate()
        {
            if (!isFlowing || target == null) return;

            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothSpeed);
            transform.position = smoothedPosition;
            transform.LookAt(target);
        }

        public void StartFlow(Transform character)
        {
            target = character;
            isFlowing = true;
        }

        public void StopFlow()
        {
            isFlowing = false;
        }
    }
}
