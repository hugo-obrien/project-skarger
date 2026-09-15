using _Project.Scripts.Combat;
using UnityEngine;

namespace _Project.Scripts.Units.State
{
    public class UnitMeleeAttackState : UnitAttackState
    {
        public UnitMeleeAttackState(Unit unit, UnitCombat combat) : base(unit, combat) { }
        
        protected override float AttackCooldown => unit.Stats.combat.meleeAttackCooldown;

        public override void Enter()
        {
            Debug.Log($"{unit.name} transitions to UnitMeleeAttackState");
            attackTimer = 0f;
            unit.SetCombatMode(AttackType.Melee);
        }

        protected override bool SuitableDistance(Vector3 targetPosition)
        {
            float distance = Vector3.Distance(unit.transform.position, targetPosition);
            if (distance > unit.Stats.combat.meleeRange)
            {
                unit.MoveTo(targetPosition);
                return false;
            }

            return true;
        }

        protected override void PerformAttack(Unit target)
        {
            unit.PlayAttackAnimation();
            target.TakeDamage(unit.Stats.combat.meleeDamage);
        }
    }
}