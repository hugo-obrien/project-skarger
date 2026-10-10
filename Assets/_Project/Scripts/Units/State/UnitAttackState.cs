using _Project.Scripts.Combat;
using _Project.Scripts.Units.Components;
using UnityEngine;

namespace _Project.Scripts.Units.State
{
    public abstract class UnitAttackState : UnitState
    {
        protected float attackTimer;

        protected readonly WeaponProfile weaponProfile;
        protected abstract AttackType AttackType { get; }

        protected UnitAttackState(Unit unit, UnitCombat combat, WeaponProfile weaponProfile) : base(unit, combat)
        {
            this.weaponProfile = weaponProfile ?? new WeaponProfile();
        }

        protected virtual float AttackCooldown => weaponProfile.attackCooldown;

        protected abstract bool SuitableDistance(Vector3 distance);
        protected abstract void PerformAttack(Unit target);

        public override void Enter()
        {
            attackTimer = 0f;
            unit.SetCombatMode(AttackType);
        }

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