using System;
using _Project.Scripts.Units;
using _Project.Scripts.Units.State;
using NUnit.Framework;
using UnityEngine;

namespace _Project.Scripts.Combat
{
    [RequireComponent(typeof(Unit))]
    public class UnitCombat : MonoBehaviour
    {
        private Unit unit;
        private UnitStateMachine stateMachine;
        private Unit target;
        
        public bool IsActive { get; private set; }
        public Unit Target => target;

        public bool HasValidTarget => target != null && !target.IsDead;

        private void Awake()
        {
            unit = GetComponent<Unit>();
            stateMachine = new UnitStateMachine();
        }

        private void Update()
        {
            if (!IsActive || unit.IsDead) return;
            stateMachine.Tick(Time.deltaTime);
        }

        public void Attack(Unit newTarget, bool force = true)
        {
            if (newTarget == null || newTarget.IsDead) return;
            if (!force && IsActive && HasValidTarget) return;

            target = newTarget;
            IsActive = true;

            if (unit.Stats.combat.isRanged)
            {
                stateMachine.ChangeState(new UnitRangedAttackState(unit, this));
            }
            else
            {
                stateMachine.ChangeState(new UnitMeleeAttackState(unit, this));
            }
        }

        public void MoveTo(Vector3 position)
        {
            IsActive = false;
            target = null;
            if (unit.MoveTo(position))
            {
                stateMachine.ChangeState(new UnitMoveState(unit, this));
            }
        }

        public void StopCombat()
        {
            IsActive = false;
            target = null;
            stateMachine.ChangeState(new UnitIdleState(unit, this));
        }
    }
}