using System;
using _Project.Scripts.Utils;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.UI
{
    public class SpeechBubble : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textLabel;
        [SerializeField] private float lifetime = 4f;

        private Camera mainCamera;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    LogUtil.Warn("SpeechBubble", "LateUpdate", "Camera not set");
                    return;
                }
            }
            
            Debug.Log("SpeechBubble.LateUpdate: rotate to camera");
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                mainCamera.transform.rotation * Vector3.up);
        }

        public void Show(string message, float duration = -1f)
        {
            Debug.Log($"SpeechBubble.Show: {message}");
            if (textLabel == null)
            {
                LogUtil.Warn("SpeechBubble", "Show", "TextLabel not set");
                return;
            }

            textLabel.text = message;
            gameObject.SetActive(true);

            if (duration > 0f)
            {
                lifetime = duration;
                Destroy(gameObject, lifetime);
            }
        }
    }
}