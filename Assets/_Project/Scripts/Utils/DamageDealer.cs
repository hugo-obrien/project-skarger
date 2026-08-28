using _Project.Scripts.Units;
using UnityEngine;

namespace _Project.Scripts.Utils {
    public class DamageDealer : MonoBehaviour {
        public float damage = 50f;

        private void Update() {
            if (Input.GetKeyDown(KeyCode.X)) {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit)) {
                    Unit unit = hit.collider.GetComponentInParent<Unit>();
                    if (unit && !unit.IsDead) {
                        Vector3 dir = (unit.transform.position - hit.point).normalized;
                        unit.TakeDamage(damage, dir);
                    }
                }
            }
        }
    }
}