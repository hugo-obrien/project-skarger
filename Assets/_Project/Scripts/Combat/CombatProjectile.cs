using UnityEngine;

namespace _Project.Scripts.Combat {
    
    /// <summary>
    /// Префаб снаряда для дальних атак.
    /// Движется к цели, наносит урон при попадании, уничтожается после lifetime.
    /// </summary>
    public class CombatProjectile : MonoBehaviour {
        
        [Header("References")]
        [SerializeField] private Transform visualTransform;
        [SerializeField] private float rotationSpeed = 720f;
        
        [Header("Damage")]
        [SerializeField] private float damage = 10f;
        
        private Vector3 direction;
        private float speed;
        private float lifetime;
        private float elapsedLifetime;
        
        private bool hasHitTarget;
        
        public void Initialize(Vector3 spawnPosition, Vector3 targetDirection, float projectileSpeed, 
                               float projLifetime, float projDamage) {
            transform.position = spawnPosition;
            direction = targetDirection.normalized;
            speed = projectileSpeed;
            lifetime = projLifetime;
            elapsedLifetime = 0f;
            damage = projDamage;
            hasHitTarget = false;
            
            if (visualTransform != null) {
                visualTransform.rotation = Quaternion.LookRotation(direction);
            }
        }
        
        private void Update() {
            if (hasHitTarget) return;
            
            // Движение вперёд
            transform.Translate(direction * speed * Time.deltaTime, Space.World);
            
            // Вращение для визуального эффекта
            if (visualTransform != null) {
                visualTransform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
            }
            
            // Проверка времени жизни
            elapsedLifetime += Time.deltaTime;
            if (elapsedLifetime >= lifetime) {
                Destroy(gameObject);
            }
        }
        
        private void OnTriggerEnter(Collider other) {
            if (hasHitTarget) return;
            
            // Проверяем, не попали ли мы в спавнер или в себя
            if (other.CompareTag("Projectile") || other.GetComponentInParent<CombatProjectile>() != null) {
                return;
            }
            
            hasHitTarget = true;
            
            // Пытаемся получить Unit из того, во что попали
            var unit = other.GetComponentInParent<Units.Unit>();
            if (unit != null && !unit.IsDead) {
                unit.TakeDamage(damage, direction);
            }
            
            // Здесь можно добавить визуальный эффект попадания (VFX)
            // Например: Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            
            Destroy(gameObject, 0.1f);
        }
    }
}
