using _Project.Scripts.Combat;
using _Project.Scripts.Units.Stats;
using _Project.Scripts.Utils;
using UnityEngine;

namespace _Project.Scripts.Units.State
{
    public class UnitRangedAttackState : UnitAttackState
    {
        public UnitRangedAttackState(Unit unit, UnitCombat combat) : base(unit, combat) { }

        protected override float AttackCooldown => unit.Stats.combat.rangedAttackCooldown;
        
        public override void Enter()
        {
            attackTimer = 0f;
            unit.SetCombatMode(AttackType.Ranged);
        }

        protected override bool SuitableDistance(Vector3 targetPosition)
        {
            float distance = Vector3.Distance(unit.transform.position, targetPosition);
            if (distance < unit.Stats.combat.rangedMinRange)
            {
                Vector3 away = (unit.transform.position - targetPosition).normalized;
                unit.MoveTo(unit.transform.position + away * 2f);
                return false;
            }

            if (distance > unit.Stats.combat.rangedMaxRange)
            {
                unit.MoveTo(targetPosition);
                return false;
            }

            return true;
        }

        protected override void PerformAttack(Unit target)
        {
            unit.PlayAttackAnimation();
            SpawnProjectile(target, unit.Stats.combat);
        }

        private void SpawnProjectile(Unit target, CombatStats stats)
        {
            if (stats.projectilePrefab == null)
            {
                LogUtil.Warn("UnitRangedAttackState", "SpawnProjectile", "Projectile prefab is null");
                target.TakeDamage(stats.rangedDamage);
                return;
            }

            Vector3 spawnPos = unit.transform.position + Vector3.up * 1.5f;
            Projectile projectile = Object.Instantiate(stats.projectilePrefab, spawnPos, Quaternion.identity);
            projectile.Launch(target, stats);
        }
    }
}