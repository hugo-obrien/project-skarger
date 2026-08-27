using UnityEngine;

namespace _Project.Scripts.UI {
    public class UnitSelectionVisual : MarkerVisual {
        [SerializeField] [Min(0f)] private float groundOffset = 0.03f;
        [SerializeField] [Min(0.1f)] private float raycastHeight = 2f;
        [SerializeField] [Min(0.1f)] private float raycastDistance = 2f;

        [SerializeField] private bool alignToGround = true;
        [SerializeField] private LayerMask groundMask = 1;

        private Transform target;
        private bool visible;

        private void LateUpdate() {
            if (!visible || !target) {
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
            base.ApplyColor(color);
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
    }
}