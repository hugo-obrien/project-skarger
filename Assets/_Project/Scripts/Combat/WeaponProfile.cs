using System;
using _Project.Scripts.Units.Stats;
using UnityEngine;

namespace _Project.Scripts.Combat
{
    /// <summary>
    /// Stub for the future equipment system.
    ///
    /// Describes a single weapon "profile": all attack parameters that used to be
    /// hardcoded inside <see cref="CombatStats"/> (min/max range, attack cooldown,
    /// projectile prefab, projectile speed and hit radius).
    ///
    /// The idea (DIP/OCP): combat states must not know where the numbers come from.
    /// Today the values are provided by a serialized stub (<see cref="DefaultWeaponProvider"/>),
    /// later they will be provided by a real equipment/inventory system that builds
    /// the profile from the weapon the unit is currently wielding — without touching
    /// the state machine code.
    /// </summary>
    [Serializable]
    public class WeaponProfile
    {
        [Header("Attack")] 
        [Min(0f)] public float minRange = 0f;
        [Min(0f)] public float maxRange = 20f;
        [Min(0f)] public float damage = 10f;
        [Min(0f)] public float attackCooldown = 1f;

        [Tooltip("If true, attack are performed via a projectile instead of an instant melee hit")]
        public bool usesProjectile = false;

        [Header("Projectile (used when usesProjectile == true)")]
        public Projectile projectilePrefab;
        [Min(0f)] public float projectileSpeed = 20f;
        [Min(0f)] public float projectileHitRadius = 0.5f;

        [Header("Hit Effect")] public GameObject hitEffectPrefab;
        [Min(0f)] public float hitEffectLifetime = 2f;

        public static WeaponProfile CreateDefault() => new();
    }
}