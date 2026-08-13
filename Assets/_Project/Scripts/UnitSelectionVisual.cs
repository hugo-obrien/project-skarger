using System;
using UnityEngine;

namespace _Project.Scripts {
    public class UnitSelectionVisual : MonoBehaviour {
        [SerializeField] private MeshRenderer markerRenderer;

        [SerializeField] [Min(0f)] private float groundOffset = 0.03f;
        [SerializeField] [Min(0.1f)] private float raycastHeight = 2f;
        [SerializeField] [Min(0.1f)] private float raycastDistance = 2f;

        [SerializeField] private bool alignToGround = true;
        [SerializeField] private LayerMask groundMask = 1;

        private Transform target;
        private bool visible;

        private MaterialPropertyBlock materialPropertyBlock;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private void Reset() {
            markerRenderer = GetComponentInChildren<MeshRenderer>(true);
        }

        private void LateUpdate() {
            if (!visible || target == null) {
                return;
            }

            if (alignToGround && TryGetGround(target.position, out RaycastHit hit)) {
                transform.position = hit.point + hit.normal * groundOffset;
                transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            } else {
                transform.position = target.position + Vector3.up * groundOffset;
                transform.rotation = Quaternion.identity;
            }
        }

        public void Show(Transform newTarget, Color color) {
            target = newTarget;
            visible = true;

            gameObject.SetActive(true);
            SetColor(color);
        }

        public void Hide() {
            target = null;
            visible = false;

            gameObject.SetActive(false);
        }

        private bool TryGetGround(Vector3 from, out RaycastHit hit) {
            return Physics.Raycast(
                from + Vector3.up * raycastHeight,
                Vector3.down,
                out hit,
                raycastHeight + raycastDistance,
                groundMask,
                QueryTriggerInteraction.Ignore
            );
        }

        private void SetColor(Color color) {
            if (markerRenderer == null) return;

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