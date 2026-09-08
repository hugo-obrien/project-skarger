using UnityEngine;

namespace _Project.Scripts.UI {
    /// <summary>
    /// Visual component for displaying a dialogue bubble above a unit.
    /// </summary>
    public class DialogueBubble : MonoBehaviour {
        [Header("References")]
        [SerializeField] private MeshRenderer bubbleRenderer;
        [SerializeField] private Transform pivot;
        
        [Header("Settings")]
        [SerializeField] private float heightOffset = 2.5f;
        [SerializeField] private float fadeDuration = 0.3f;
        [SerializeField] private float displayDuration = 2f;
        
        [Header("Text (Optional)")]
        [SerializeField] private TMPro.TextMeshPro textMesh;
        
        private MaterialPropertyBlock materialPropertyBlock;
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        
        private float displayTimeLeft;
        private bool isFadingOut;
        private Color baseColor = Color.white;
        
        private void Reset() {
            bubbleRenderer = GetComponentInChildren<MeshRenderer>(true);
            if (transform.childCount > 0) {
                pivot = transform.GetChild(0);
            }
        }
        
        private void Update() {
            if (displayTimeLeft <= 0f) return;
            
            displayTimeLeft -= Time.deltaTime;
            
            if (displayTimeLeft <= 0f && !isFadingOut) {
                StartFadeOut();
            }
            
            if (isFadingOut && displayTimeLeft < -fadeDuration) {
                Destroy(gameObject);
            }
        }
        
        public void Setup(Vector3 worldPosition, string message, Color color, float duration = 2f) {
            transform.position = worldPosition + Vector3.up * heightOffset;
            
            baseColor = color;
            displayDuration = duration;
            displayTimeLeft = displayDuration;
            isFadingOut = false;
            
            if (textMesh != null) {
                textMesh.text = message;
                textMesh.color = Color.black;
            }
            
            ApplyColor(1f);
            gameObject.SetActive(true);
        }
        
        private void StartFadeOut() {
            isFadingOut = true;
            displayTimeLeft = -fadeDuration;
        }
        
        private void LateUpdate() {
            if (pivot == null) return;
            
            Vector3 lookTarget = transform.position + (transform.position - Camera.main?.transform.position ?? Vector3.forward);
            transform.LookAt(lookTarget, Vector3.up);
        }
        
        private void ApplyColor(float alphaMultiplier) {
            if (!bubbleRenderer) return;
            
            materialPropertyBlock ??= new MaterialPropertyBlock();
            bubbleRenderer.GetPropertyBlock(materialPropertyBlock);
            
            Color color = baseColor;
            color.a *= alphaMultiplier;
            
            materialPropertyBlock.SetColor(ColorId, color);
            materialPropertyBlock.SetColor(BaseColorId, color);
            bubbleRenderer.SetPropertyBlock(materialPropertyBlock);
            
            if (textMesh != null) {
                Color textColor = Color.black;
                textColor.a *= alphaMultiplier;
                textMesh.color = textColor;
            }
        }
    }
}
