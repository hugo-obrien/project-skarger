using System;
using _Project.Scripts.Units;
using _Project.Scripts.Units.Stats;
using UnityEngine;

namespace _Project.Scripts.Combat
{
    public class Projectile : MonoBehaviour
    {
        private Unit target;
        private CombatStats stats;
        private bool hasHit;

        private void Update()
        {
            if (target == null || target.IsDead || hasHit)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 targetPoint = target.transform.position + Vector3.up * 1f;
            Vector3 direction = (targetPoint - transform.position).normalized;

            transform.position += direction * stats.projectileSpeed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(direction);

            if (Vector3.Distance(transform.position, targetPoint) <= stats.projectileHitRadius)
            {
                hasHit = true;
                target.TakeDamage(stats.rangedDamage);
                SpawnHitEffect();
                Destroy(gameObject);
            }
        }

        public void Launch(Unit target, CombatStats stats)
        {
            this.target = target;
            this.stats = stats;
        }

        private void SpawnHitEffect()
        {
            if (stats.hitEffectPrefab == null) return;

            GameObject effect = Instantiate(stats.hitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, stats.hitEffectTime);
        }
    }
}