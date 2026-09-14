using System;
using UnityEngine;

namespace _Project.Scripts.Combat {
    
    public enum CombatState {
        None,           // Не в бою
        Alerted,        // Заметил врага, но ещё не атакует
        InCombat,       // Активный бой
        Fleeing         // Отступление
    }
    
    [Serializable]
    public class CombatStats {
        [Header("Melee Combat")]
        [Min(0f)] public float meleeRange = 1.5f;
        [Min(0f)] public float meleeDamage = 10f;
        [Min(0.1f)] public float attackSpeed = 1.5f; // атак в секунду
        [Min(0f)] public float attackCooldown = 0f;  // дополнительная задержка между атаками
        
        [Header("Ranged Combat")]
        [Min(0f)] public float rangedMinRange = 3f;
        [Min(0f)] public float rangedMaxRange = 15f;
        [Min(0f)] public float rangedDamage = 8f;
        [Min(0.1f)] public float rangedAttackSpeed = 1f;
        [Min(0f)] public float rangedAttackCooldown = 0f;
        
        [Header("Projectile")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform projectileSpawnPoint;
        [Min(0f)] public float projectileSpeed = 10f;
        [Min(0f)] public float projectileLifetime = 5f;
        
        [Header("Behavior")]
        [Min(0f)] public float aggroRange = 10f;
        [Min(0f)] public float deaggroRange = 15f;
        [Min(0f)] public float chaseRange = 20f;
        
        public GameObject ProjectilePrefab => projectilePrefab;
        public Transform ProjectileSpawnPoint => projectileSpawnPoint;
    }
}
