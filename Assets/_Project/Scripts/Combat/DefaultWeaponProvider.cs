using UnityEngine;

namespace _Project.Scripts.Combat
{
    /// <summary>
    /// Stub implementation of <see cref="IWeaponProvider"/> for the not-yet-existing
    /// equipment system: all weapon profiles are simply configured in the Inspector
    /// (per-unit overrides via a component on the unit, or project-wide defaults).
    ///
    /// When the real equipment system is implemented, it will replace (or feed) this
    /// component without any changes to the combat state machine.
    /// </summary>
    [DisallowMultipleComponent]
    public class DefaultWeaponProvider : MonoBehaviour, IWeaponProvider
    {
        [Header("Wielding (stub until the equipment system exists)")]
        [Tooltip("Which attack the unit performs by default. Later this will be derived from the equipped weapon")]
        [SerializeField] private AttackType defaultAttackType;

        [Header("Melee weapon profile")] [SerializeField]
        private WeaponProfile meleeProfile = new WeaponProfile
        {
            minRange = 0f,
            maxRange = 2f,
            damage = 10f,
            attackCooldown = 1f,
            usesProjectile = false
        };
        
        [Header("Ranged weapon profile")] [SerializeField]
        private WeaponProfile rangedProfile = new WeaponProfile
        {
            minRange = 3f,
            maxRange = 20f,
            damage = 4f,
            attackCooldown = 2f,
            usesProjectile = true
        };
        
        [Header("Spell weapon profile")] [SerializeField]
        private WeaponProfile spellProfile = new WeaponProfile
        {
            minRange = 3f,
            maxRange = 30f,
            damage = 20f,
            attackCooldown = 5f,
            usesProjectile = true
        };

        public WeaponProfile MeleeProfile => meleeProfile;
        public WeaponProfile RangedProfile => rangedProfile;
        public WeaponProfile SpellProfile => spellProfile;

        public AttackType CurrentAttackType => defaultAttackType;

        /// <summary>
        /// Allows the (future) equipment system or debug tooling to change what the unit wields
        /// </summary>
        public void SetDefaultAttackType(AttackType attackType)
        {
            defaultAttackType = attackType;
        }

        public WeaponProfile GetProfile(AttackType attackType)
        {
            switch (attackType)
            {
                case AttackType.Melee: return meleeProfile;
                case AttackType.Ranged: return rangedProfile;
                case AttackType.Spell: return spellProfile;
                default: return null;
            }
        }
    }
}