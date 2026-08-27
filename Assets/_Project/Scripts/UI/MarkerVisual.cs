using _Project.Scripts.Utils;
using UnityEngine;

namespace _Project.Scripts.UI {
    public abstract class MarkerVisual : MonoBehaviour {
        [SerializeField] protected MeshRenderer markerRenderer;

        private MaterialPropertyBlock materialPropertyBlock;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        protected void Reset() {
            markerRenderer = GetComponentInChildren<MeshRenderer>(true);
        }

        protected void Awake() {
            if (markerRenderer == null) {
                LogUtil.Warn("MarkerVisual", "Awake", "MarkerRenderer is null");
            }
        }

        protected void ApplyColor(Color color) {
            if (!markerRenderer) {
                LogUtil.Warn("MarkerVisual", "ApplyColor", "MarkerRenderer is null");
                return;
            }

            materialPropertyBlock ??= new MaterialPropertyBlock();

            markerRenderer.GetPropertyBlock(materialPropertyBlock);

            materialPropertyBlock.SetColor(ColorId, color);
            materialPropertyBlock.SetColor(BaseColorId, color);

            markerRenderer.SetPropertyBlock(materialPropertyBlock);
        }
    }
}