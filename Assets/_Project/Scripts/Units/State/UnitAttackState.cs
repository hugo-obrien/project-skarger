using _Project.Scripts.Combat;
using _Project.Scripts.Units.Stats;
using UnityEngine;

namespace _Project.Scripts.Units.State
{
    public abstract class UnitAttackState : UnitState
    {
        protected float attackTimer;
        
        protected UnitAttackState(Unit unit, UnitCombat combat) : base(unit, combat) { }
        
        protected abstract float AttackCooldown { get; }

        protected abstract bool SuitableDistance(Vector3 distance);
        protected abstract void PerformAttack(Unit target);

        public override void Tick(float deltaTime)
        {
            Unit target = combat.Target;
            if (target == null || target.IsDead)
            {
                combat.StopCombat();
                return;
            }
            
            if (!SuitableDistance(target.transform.position))
            {
                return;
            }
            
            unit.Stop();
            unit.RotateTowards(target.transform.position);

            attackTimer -= deltaTime;
            if (attackTimer <= 0f)
            {
                PerformAttack(target);
                attackTimer = AttackCooldown;
            }
        }
        
        public override void Exit()
        {
            unit.SetCombatMode(AttackType.None);
        }
    }
}