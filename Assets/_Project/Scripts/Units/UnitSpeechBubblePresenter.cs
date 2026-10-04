using _Project.Scripts.UI;
using _Project.Scripts.Utils;
using UnityEngine;

namespace _Project.Scripts.Units {

    /// <summary>
    /// Ответственность: показ речевых пузырей над юнитом (SRP).
    /// Unit делегирует ему эту задачу и не знает деталей Instantiate/якорей (DIP).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UnitSpeechBubblePresenter : MonoBehaviour {

        private const float DefaultAnchorHeight = 2f;

        [SerializeField] private SpeechBubble speechBubblePrefab;
        [SerializeField] private Transform speechBubbleAnchor;

        private void Awake() {
            if (speechBubblePrefab == null) {
                LogUtil.Warn(nameof(UnitSpeechBubblePresenter), nameof(Awake), "SpeechBubble prefab not set");
            }
        }

        public void Show(string text, float duration = 4f) {
            Debug.Log($"Show speech bubble: {text}");

            if (speechBubblePrefab == null) {
                LogUtil.Warn(nameof(UnitSpeechBubblePresenter), nameof(Show), "SpeechBubble prefab not set");
                return;
            }

            Vector3 spawnPosition = speechBubbleAnchor != null
                ? speechBubbleAnchor.position
                : transform.position + Vector3.up * DefaultAnchorHeight;

            SpeechBubble bubble = Instantiate(speechBubblePrefab, spawnPosition, Quaternion.identity);

            if (speechBubbleAnchor != null) {
                bubble.transform.SetParent(speechBubbleAnchor, worldPositionStays: true);
            }

            bubble.Show(text, duration);
        }
    }
}
