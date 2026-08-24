using UnityEngine;

namespace _Project.Scripts {
    public abstract class MarkerVisual : MonoBehaviour {
        [SerializeField] protected MeshRenderer markerRenderer;

        protected MaterialPropertyBlock materialPropertyBlock;

        protected static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        protected static readonly int ColorId = Shader.PropertyToID("_Color");

        protected void Reset() {
            markerRenderer = GetComponentInChildren<MeshRenderer>(true);
        }

        protected void Awake() {
            if (markerRenderer == null) {
                Debug.LogWarning($"{GetType().Name}.ApplyColor(): markRenderer is null");
            }
        }

        protected void ApplyColor(Color color) {
            if (markerRenderer == null) {
                Debug.LogWarning($"{GetType().Name}.ApplyColor(): markRenderer is null");
                return;
            }

            if (materialPropertyBlock == null) {
                materialPropertyBlock = new MaterialPropertyBlock();
            }

            markerRenderer.GetPropertyBlock(materialPropertyBlock);

            materialPropertyBlock.SetColor(ColorId, color);
            materialPropertyBlock.SetColor(BaseColorId, color);

            markerRenderer.SetPropertyBlock(materialPropertyBlock);
        }
    }
}