using System;
using UnityEngine;

namespace _Project.Scripts.UI {
    public class SelectionBoxOverlay : MonoBehaviour {
        [SerializeField] private Color fillColor = new Color(0f, 1f, 0f, 0.15f);
        [SerializeField] private Color borderColor = new Color(0f, 1f, 0f, 0.9f);
        [SerializeField] [Min(1f)] private float borderThickness = 2f;

        private Rect screenRect;
        private bool isVisible;

        public void Show(Rect rect) {
            screenRect = rect;
            isVisible = true;
        }

        public void Hide() {
            isVisible = false;
        }

        private void OnGUI() {
            if (!isVisible) return;
            if (Event.current.type != EventType.Repaint) return;

            Rect guiRect = ToGuiRect(screenRect);
            if (guiRect.width <= 0f || guiRect.height <= 0) return;

            GUI.color = fillColor;
            GUI.DrawTexture(guiRect, Texture2D.whiteTexture);

            GUI.color = borderColor;
            GUI.DrawTexture(new Rect(guiRect.xMin, guiRect.yMin, guiRect.width, borderThickness), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(guiRect.xMin, guiRect.yMax - borderThickness, guiRect.width, borderThickness), 
                Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(guiRect.xMin, guiRect.yMin, borderThickness, guiRect.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(guiRect.xMax - borderThickness, guiRect.yMin, borderThickness, guiRect.height), 
                Texture2D.whiteTexture);

            GUI.color = Color.white;
        }

        private Rect ToGuiRect(Rect screen) {
            return new Rect(screen.xMin, Screen.height - screen.yMax, screen.width, screen.height);
        }
    }
}
