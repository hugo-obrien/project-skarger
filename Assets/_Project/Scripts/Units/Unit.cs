using System;
using _Project.Scripts.Combat;
using _Project.Scripts.Movement;
using _Project.Scripts.UI;
using _Project.Scripts.Units.Components;
using _Project.Scripts.Units.Factions;
using _Project.Scripts.Units.Stats;
using _Project.Scripts.Utils;
using UnityEngine;

namespace _Project.Scripts.Units {
    
    /// <summary>
    /// Unit facade: aggregates specialized components and provides a unified API for
    /// external systems (state management, AI, console, input controllers). 
    /// </summary>
    
    [RequireComponent(typeof(UnitMovement))]
    [RequireComponent(typeof(UnitHealth))]
    [RequireComponent(typeof(UnitAnimatorController))]
    [RequireComponent(typeof(UnitSelectionVisualController))]
    [RequireComponent(typeof(UnitDeathHandler))]
    public class Unit : MonoBehaviour, IDamageDealer {
        
        [Header("Faction")] [SerializeField] private UnitFaction faction = UnitFaction.User;
        [Header("Stats")] [SerializeField] private UnitStats stats;

        private UnitMovement movement;
        private UnitHealth health;
        private UnitAnimatorController animatorController;
        private UnitSpeechBubblePresenter speechBubblePresenter;
        
        private bool isSelected;
        private bool isInCombat;
        private AttackType currentAttackType;
        private Vector3? lastImpactDirection;
        
        public UnitStats Stats => stats;
        public UnitFaction Faction => faction;
        
        public bool IsPlayerControlled => faction == UnitFaction.User;
        public bool IsSelected => isSelected;
        public bool IsRunning => movement != null && movement.IsRunning;
        public bool IsDead => health != null && health.IsDead;
        public bool IsInCombat => isInCombat;
        
        public float CurrentHealth => health != null ? health.CurrentHealth : 0f;
        public float MaxHealth => health != null ? health.MaxHealth : 0f;

        public Vector3? LastImpactDirection => lastImpactDirection;

        public event Action<Unit> Destroyed;
        public event Action<IDamageDealer> ReceivedDamage;
        public event Action Died;
        public event Action SelectionChanged;
        public event Action FactionChanged;
        public event Action CombatModeChanged;
        public event Action<string> OnAnimationEvent;
        
        private void Awake()
        {
            movement = GetComponent<UnitMovement>();
            health = GetComponent<UnitHealth>();
            animatorController = GetComponent<UnitAnimatorController>();
            speechBubblePresenter = GetComponent<UnitSpeechBubblePresenter>();

            if (movement == null)
            {
                Debug.Log($"{nameof(Unit)} requires {nameof(UnitMovement)} on the same GameObject");
            }
            
            if (health == null)
            {
                Debug.Log($"{nameof(Unit)} requires {nameof(UnitHealth)} on the same GameObject");
            }
            
            health.Initialize(stats.maxHealth);
            movement.Initialize();
            animatorController.Initialize();
        }
        
        private void Update() {
            if (IsDead) return;
            
            movement.Tick();
            
            RotateTowardsMovementDirection();
            UpdateAnimation();
        }
        
        private void OnDestroy() {
            Destroyed?.Invoke(this);
        }

        private void OnEnable() {
            UnitRegistry.Register(this);
        }

        private void OnDisable() {
            UnitRegistry.Unregister(this);
        }
        
        public void SetSelected(bool value) {
            if (isSelected == value) return;

            isSelected = value;
            SelectionChanged?.Invoke();
        }

        public void SetFaction(UnitFaction newFaction)
        {
            if (IsDead || faction == newFaction) return;

            faction = newFaction;
            FactionChanged?.Invoke();
        }
        
        public bool MoveTo(Vector3 worldPosition)
        {
            return movement.MoveTo(worldPosition);
        }

        public void Stop()
        {
            movement.Stop();
        }

        public bool HasReachedDestination(float tolerance = 0.2f)
        {
            return movement.HasReachedDestination(tolerance);
        }

        public void SetMovementMode(MovementMode mode)
        {
            movement.SetMovementMode(mode);
        }
        
        public void SetCombatMode(AttackType attackType)
        {
            if (currentAttackType == attackType)
            {
                return;
            }

            currentAttackType = attackType;
            isInCombat = attackType != AttackType.None;
            
            animatorController.SetCombatState(isInCombat, attackType);
            
            CombatModeChanged?.Invoke();
        }
        
        public void PlayAttackAnimation()
        {
            animatorController.PlayAttack();
        }
        
        public void RotateTowards(Vector3 position)
        {
            Vector3 direction = position - transform.position;
            RotateTowardsInternal(direction, Time.deltaTime * stats.turnSpeed);
        }
        
        public void TakeDamage(float amount, IDamageDealer causer, Vector3? impactDirection = null) {
            if (amount <= 0f) return;

            lastImpactDirection = impactDirection;
            health.TakeDamage(amount, impactDirection);

            if (health.IsDead)
            {
                movement.SetDead(true);
                
                Died?.Invoke();
                SetSelected(false);
                return;
            }

            if (causer != null)
            {
                ReceivedDamage?.Invoke(causer);
            }
        }

        public void Heal(float amount)
        {
            if (amount <= 0f) return;
            health.Heal(amount);
        }
        
        public void SaySomething(string message) {
            LogUtil.Info("Unit", "SaySomething", message);
            ShowSpeechBubble(message);
        }
        
        public void ShowSpeechBubble(string text, float duration = 4f)
        {
            if (speechBubblePresenter == null)
            {
                LogUtil.Warn("Unit", "ShowSpeechBubble", "SpeechBubblePresenter is not set");
                return;
            }
            
            speechBubblePresenter.Show(text, duration);
        }

        public void DispatchAnimationEvent(string eventName)
        {
            Debug.Log("Unit.DispatchAnimationEvent() called");
            OnAnimationEvent?.Invoke(eventName);
        }

        private void RotateTowardsMovementDirection() {
            if (movement == null || !movement.IsMoving) return;

            Vector3 direction = movement.DesiredVelocity;
            if (direction.sqrMagnitude < 0.001f)
            {
                direction = movement.Velocity;
            }

            RotateTowardsInternal(direction, Time.deltaTime * stats.turnSpeed);
        }

        private void RotateTowardsInternal(Vector3 direction, float slerpT)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.001f) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, slerpT);
        }

        private void UpdateAnimation()
        {
            if (animatorController == null) return;

            float speedValue = movement != null && movement.IsMoving ? movement.Velocity.magnitude : 0f;
            animatorController.SetSpeed(speedValue);
        }
    }
}