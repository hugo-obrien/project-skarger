using UnityEngine;

namespace _Project.Scripts.Units {
    public class UnitRagdoll : MonoBehaviour {
        [SerializeField] private Animator animator;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Impulse force applied to hips on death")]
        private float impactForce = 2f;

        private Rigidbody[] ragdollRigidbodies;

        private void Awake() {
            if (animator == null) {
                animator = GetComponent<Animator>();
            }

            ragdollRigidbodies = GetComponentsInChildren<Rigidbody>(true);

            foreach (var rb in ragdollRigidbodies) {
                rb.isKinematic = true;
                rb.useGravity = false;
            }
        }

        public void Activate(Vector3? impactDirection = null) {
            if (animator != null) {
                animator.Update(0f);
                animator.enabled = false;
            }

            foreach (var rb in ragdollRigidbodies) {
                rb.position = rb.transform.position;
                rb.rotation = rb.transform.rotation;
                
                rb.useGravity = true;
                rb.isKinematic = false;
                
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            if (impactDirection.HasValue && ragdollRigidbodies.Length > 0) {
                Rigidbody hips = FindHipsRigidbody();
                if (hips != null) {
                    hips.AddForce(impactDirection.Value * impactForce, ForceMode.Impulse);
                }
            }
        }

        private Rigidbody FindHipsRigidbody() {
            foreach (var rb in ragdollRigidbodies) {
                string rbName = rb.gameObject.name.ToLower();
                if (rbName.Contains("hips") || rbName.Contains("pelvis") || rbName.Contains("root")) {
                    return rb;
                }
            }

            return ragdollRigidbodies.Length > 0 ? ragdollRigidbodies[0] : null;
        }
    }
}
