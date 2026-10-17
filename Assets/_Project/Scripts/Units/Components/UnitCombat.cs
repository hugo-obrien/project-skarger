using _Project.Scripts.Combat;
using _Project.Scripts.Units.State;
using UnityEngine;

namespace _Project.Scripts.Units.Components
{
    [RequireComponent(typeof(Unit))]
    public class UnitCombat : MonoBehaviour
    {
         
        
        private Unit unit;
        private UnitStateMachine stateMachine;
        private Unit target;

        private IWeaponProvider weaponProvider;
        
        public bool IsActive { get; private set; }
        public Unit Target => target;

        public bool HasValidTarget => target != null && !target.IsDead;

        private void Awake()
        {
            unit = GetComponent<Unit>();
            stateMachine = new UnitStateMachine();
            ResolveWeaponProvider();
        }

        private void Update()
        {
            if (!IsActive || unit.IsDead) return;
            stateMachine.Tick(Time.deltaTime);
        }

        public void SetWeaponProvider(IWeaponProvider provider)
        {
            if (provider == null) return;
            weaponProvider = provider;
        }

        public void Attack(Unit newTarget, bool force = true)
        {
            Debug.Log($"{unit.name} start attack");
            if (newTarget == null || newTarget.IsDead) return;
            if (!force && IsActive && HasValidTarget) return;

            target = newTarget;
            IsActive = true;
            
            stateMachine.ChangeState(CreateAttackState());
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

        private void ResolveWeaponProvider()
        {
            weaponProvider = GetComponent<IWeaponProvider>();

            if (weaponProvider == null)
            {
                Debug.LogWarning($"{nameof(UnitCombat)} on {unit.name}: no {nameof(IWeaponProvider)} found, " +
                                 "creating an in-editor default one.");
                
                
                var provider = gameObject.AddComponent<DefaultWeaponProvider>();
                provider.SetDefaultAttackType(AttackType.Melee);
                weaponProvider = provider;
            }
        }

        

        private UnitState CreateAttackState()
        {
            
            
            AttackType attackType = weaponProvider.CurrentAttackType;

            switch (attackType)
            {
                case AttackType.Melee: return new UnitMeleeAttackState(unit, this, weaponProvider.MeleeProfile);
                case AttackType.Ranged: return new UnitRangedAttackState(unit, this, weaponProvider.RangedProfile);
                case AttackType.Spell: return new UnitSpellAttackState(unit, this, weaponProvider.SpellProfile);
                default:
                {
                    Debug.LogError($"{nameof(UnitCombat)} on {unit.name}: unsupported attack type {attackType}");
                    return null;
                }
            }
        }

    }
}