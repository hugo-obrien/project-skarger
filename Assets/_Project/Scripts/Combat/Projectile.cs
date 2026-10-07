using System;
using _Project.Scripts.Units;
using _Project.Scripts.Units.Stats;
using UnityEngine;

namespace _Project.Scripts.Combat
{
    public class Projectile : MonoBehaviour
    {
        private Unit target;
        private WeaponProfile profile;
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

            transform.position += direction * (profile.projectileSpeed * Time.deltaTime);
            transform.rotation = Quaternion.LookRotation(direction);

            if (Vector3.Distance(transform.position, targetPoint) <= profile.projectileHitRadius)
            {
                hasHit = true;
                target.TakeDamage(profile.damage);
                SpawnHitEffect();
                Destroy(gameObject);
            }
        }

        public void Launch(Unit target, WeaponProfile profile)
        {
            this.target = target;
            this.profile = profile;
        }

        private void SpawnHitEffect()
        {
            if (profile == null || profile.hitEffectPrefab == null) return;

            GameObject effect = Instantiate(profile.hitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, profile.hitEffectLifetime);
        }
    }
}