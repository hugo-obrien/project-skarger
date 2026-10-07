using _Project.Scripts.Combat;
using UnityEngine;

namespace _Project.Scripts.Units.State
{
    public class UnitMeleeAttackState : UnitAttackState
    {
        public UnitMeleeAttackState(Unit unit, UnitCombat combat, WeaponProfile weaponProfile) : base(unit, combat, weaponProfile) { }

        protected override AttackType AttackType => AttackType.Melee;

        public override void Enter()
        {
            Debug.Log($"{unit.name} transitions to UnitMeleeAttackState");
            base.Enter();
        }

        protected override bool SuitableDistance(Vector3 targetPosition)
        {
            float distance = Vector3.Distance(unit.transform.position, targetPosition);
            if (distance > weaponProfile.maxRange)
            {
                unit.MoveTo(targetPosition);
                return false;
            }

            return true;
        }

        protected override void PerformAttack(Unit target)
        {
            unit.PlayAttackAnimation();
            target.TakeDamage(weaponProfile.damage);
        }
    }
}