using System;
using _Project.Scripts.Combat;
using UnityEngine;

namespace _Project.Scripts.Units.Stats
{
    [Serializable]
    public class CombatStats
    {
        [Header("General")] 
        public bool isRanged = false;

        [Header("AI")] 
        public bool aiControlled = false;
        [Min(0.1f)] public float perceptionRadius = 15f;
        [Min(0.1f)] public float targetSearchInterval = 0.5f;
        
        [Header("Melee")] 
        [Min(0.1f)] public float meleeRange = 2.0f;
        [Min(0.1f)] public float meleeDamage = 10f;
        [Min(0.1f)] public float meleeAttackCooldown = 1.0f;
        
        [Header("Ranged")]
        [Min(0.1f)] public float rangedMinRange = 3.0f;
        [Min(0.1f)] public float rangedMaxRange = 20.0f;
        [Min(0.1f)] public float rangedDamage = 2.0f;
        [Min(0.1f)] public float rangedAttackCooldown = 2.0f;

        [Header("Projectile")] 
        public Projectile projectilePrefab;
        [Min(0.1f)] public float projectileSpeed = 20f;
        [Min(0.1f)] public float projectileHitRadius = 0.5f;
        
        [Header("Hit Effect")]
        public GameObject hitEffectPrefab;
        [Min(0.1f)] public float hitEffectTime = 2.0f;
    }
}