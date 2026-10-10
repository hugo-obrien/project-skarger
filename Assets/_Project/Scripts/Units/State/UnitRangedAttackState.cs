using _Project.Scripts.Combat;
using _Project.Scripts.Systems;
using _Project.Scripts.Units.Components;
using _Project.Scripts.Units.Stats;
using _Project.Scripts.Utils;
using UnityEngine;

namespace _Project.Scripts.Units.State
{
    public class UnitRangedAttackState : UnitAttackState
    {

        private Unit currentAttackTarget;
        private bool hasSpawnedProjectile;
        
        public UnitRangedAttackState(Unit unit, UnitCombat combat, WeaponProfile weaponProfile) 
            : base(unit, combat, weaponProfile) { }

        protected override AttackType AttackType => AttackType.Ranged;

        public override void Enter()
        {
            unit.OnAnimationEvent += HandleAnimationEvent;
            hasSpawnedProjectile = false;
            
            base.Enter();
        }

        public override void Exit()
        {
            unit.OnAnimationEvent -= HandleAnimationEvent;
            base.Exit();
        }

        protected override bool SuitableDistance(Vector3 targetPosition)
        {
            float distance = Vector3.Distance(unit.transform.position, targetPosition);
            if (distance < weaponProfile.minRange)
            {
                Vector3 away = (unit.transform.position - targetPosition).normalized;
                unit.MoveTo(unit.transform.position + away * 2f);
                return false;
            }

            if (distance > weaponProfile.maxRange)
            {
                unit.MoveTo(targetPosition);
                return false;
            }

            return true;
        }

        protected override void PerformAttack(Unit target)
        {
            currentAttackTarget = target;
            hasSpawnedProjectile = false;
            unit.PlayAttackAnimation();
        }

        protected virtual void SpawnProjectile(Unit target, WeaponProfile profile)
        {
            if (profile.projectilePrefab ==null)
            {
                Debug.LogWarning($"{nameof(UnitRangedAttackState)} of {unit.name}: profile.projectilePrefab == null");
                target.TakeDamage(profile.damage, unit);
                return;
            }
            
            Vector3 spawnPos = unit.transform.position + Vector3.up * 1.5f; // todo rework for weapon socket
            Projectile projectile = Object.Instantiate(profile.projectilePrefab, spawnPos, Quaternion.identity);
            projectile.Launch(unit, target, profile);
        }

        private void HandleAnimationEvent(string eventName)
        {
            if (eventName == "SpawnProjectile")
            {
                if (hasSpawnedProjectile) return;
                hasSpawnedProjectile = true;

                if (currentAttackTarget != null && !currentAttackTarget.IsDead)
                {
                    SpawnProjectile(currentAttackTarget, weaponProfile);
                }
            }
        }
    }
}