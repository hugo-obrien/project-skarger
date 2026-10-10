using _Project.Scripts.Combat;
using _Project.Scripts.Units.Components;
using UnityEngine;

namespace _Project.Scripts.Units.State
{
    /// <summary>
    /// Spell attack state: a ranged-style attack performed with magic.
    /// Reuses the projectile pipeline of <see cref="UnitRangedAttackState"/>, but the
    /// spell "weapon" profile (prefab, speed, radius, damage, cooldown, ranges) comes
    /// from the outside, so spells differ from ordinary arrows/bolts only by data.
    /// Has its own <see cref="AttackType.Spell"/> so animation/AI can distinguish it.
    /// </summary>
    public class UnitSpellAttackState : UnitRangedAttackState
    {
        public UnitSpellAttackState(Unit unit, UnitCombat combat, WeaponProfile weaponProfile) : base(unit, combat, weaponProfile) { }

        protected override AttackType AttackType => AttackType.Spell;

        public override void Enter()
        {
            Debug.Log($"{unit.name} transitions to UnitSpellAttackState");
            base.Enter();
        }
    }
}