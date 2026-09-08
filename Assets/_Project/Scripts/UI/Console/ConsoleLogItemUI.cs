using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.UI.Console
{
    public class ConsoleLogItemUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] public Transform logContent;
        [SerializeField] public GameObject logEntryPrefab;
        [SerializeField] public ScrollRect scrollRect;
        
        [Header("Settings")]
        [SerializeField] public int maxLogEntries = 100;
        [SerializeField] public bool autoScroll = true;

        private readonly Queue<GameObject> logEntries = new();

        public void AddLogEntry(string message, Color? color = null)
        {
            GameObject entry = Instantiate(logEntryPrefab, logContent);
            TextMeshProUGUI textComponent = entry.GetComponent<TextMeshProUGUI>();

            string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
            textComponent.text = $"[{timestamp}] {message}";

            if (color.HasValue)
            {
                textComponent.color = color.Value;
            }
            
            logEntries.Enqueue(entry);

            while (logEntries.Count > maxLogEntries)
            {
                GameObject oldEntry = logEntries.Dequeue();
                Destroy(oldEntry);
            }

            if (autoScroll)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }

        public void Clear()
        {
            foreach (var entry in logEntries)
            {
                Destroy(entry);
            }
            logEntries.Clear();
        }
    }
}