using System;
using _Project.Scripts.Combat;
using _Project.Scripts.UI;
using _Project.Scripts.Units.Factions;
using _Project.Scripts.Utils;
using UnityEngine;
using UnityEngine.AI;

namespace _Project.Scripts.Units {

    public enum MovementMode {
        Auto,
        ForcedWalk,
        ForcedRun
    }

    [RequireComponent(typeof(NavMeshAgent))]
    public class Unit : MonoBehaviour {
        [Header("Faction")] [SerializeField] private UnitFaction faction = UnitFaction.User;
        [Header("Stats")] [SerializeField] private UnitStats stats = new UnitStats();
        [Header("Selector")] [SerializeField] private UnitSelectionVisual selectionVisual;

        [Header("Animation")] 
        [SerializeField] private Animator animator;
        [SerializeField] private string speedParameter = "Speed";
        [SerializeField] private bool disableRootMotion = true;
        
        [Header("Death & Physics")]
        [SerializeField] private UnitRagdoll ragdoll;
        [SerializeField] private CapsuleCollider rootCollider;
        [SerializeField] private string deathTrigger = "Death";
        
        [Header("Speech Bubble")]
        [SerializeField] private SpeechBubble speechBubblePrefab;
        [SerializeField] private Transform speechBubbleAnchor;

        [Header("Combat")] 
        [SerializeField] private string attackTrigger = "Attack";
        [SerializeField] private string combatIdleTrigger = "IsInCombat";
        [SerializeField] private string attackTypeParam = "AttackType";

        private NavMeshAgent agent;
        private bool isSelected;
        private int speedParameterHash;
        private bool hasSpeedParameter;

        private MovementMode movementMode = MovementMode.Auto;
        private bool isRunning;

        private float currentHealth;
        private bool isDead;

        private int deathTriggerHash;
        private bool hasDeathTrigger;

        private SpeechBubble currentBubble;

        private bool isInCombat;
        private AttackType currentAttackType;
        
        private int attackTriggerHash;
        private bool hasAttackTrigger;
        private int combatIdleHash;
        private bool hasCombatIdle;
        private int attackTypeHash;
        private bool hasAttackType;

        public UnitStats Stats => stats;
        public UnitFaction Faction => faction;
        public bool IsPlayerControlled => faction == UnitFaction.User;
        public bool IsRunning => isRunning;
        public bool IsDead => isDead;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => stats.maxHealth;
        public bool IsInCombat => isInCombat;
        
        public event Action<Unit> Destroyed;
        public event Action<Unit> Died; 

        private void Awake() {
            agent = GetComponent<NavMeshAgent>();
            agent.updateRotation = false;

            if (ragdoll == null) {
                ragdoll = GetComponentInChildren<UnitRagdoll>(true);
            }

            SetupAnimator();
            ApplyStatsToAgent();
            currentHealth = stats.maxHealth;
        }

        private void Update() {
            if (isDead) return;

            if (!isInCombat)
            {
                UpdateMovementSpeed();
            }
            
            RotateTowardsMovementDirection();
            UpdateAnimation();
        }

        private void OnDestroy() {
            Destroyed?.Invoke(this);
        }

        private void OnEnable() {
            UnitRegistry.Register(this);
            UpdateSelectionVisual();
        }

        private void OnDisable() {
            UnitRegistry.Unregister(this);
        }

        public void SetSelected(bool value) {
            if (isSelected == value) return;

            isSelected = value;
            UpdateSelectionVisual();
        }

        public bool MoveTo(Vector3 worldPosition) {
            if (isDead || !agent || !agent.isOnNavMesh) return false;

            agent.stoppingDistance = 0.05f;
            agent.isStopped = false;

            if (NavMesh.SamplePosition(worldPosition, out NavMeshHit navHit, stats.destinationSnapDistance, NavMesh.AllAreas)) 
            {
                worldPosition = navHit.position;
            } else {
                return false;
            }

            return agent.SetDestination(worldPosition);
        }

        public void Stop() {
            if (isDead || agent == null || !agent.isOnNavMesh) return;

            agent.isStopped = true;
            agent.ResetPath();
        }

        public bool HasReachedDestination(float tolerance = 0.2f) {
            if (isDead || !agent || !agent.isOnNavMesh) return true;

            if (agent.pathPending) return false;

            return agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, tolerance);
        }

        public void SetFaction(UnitFaction newFaction) { 
            if (isDead || faction == newFaction) {
                return;
            }
            
            faction = newFaction;
            UpdateSelectionVisual();
        }

        public void SetMovementMode(MovementMode mode) {
            if (isDead) return;
            movementMode = mode;
        }

        public void TakeDamage(float amount, Vector3? impactDirection = null) {
            if (isDead || amount <= 0f) return;

            currentHealth -= amount;
            if (currentHealth <= 0f) {
                currentHealth = 0;
                Die(impactDirection);
            }
        }

        public void SaySomething(string message) {
            LogUtil.Info("Unit", "SaySomething", message);
            ShowSpeechBubble(message);
        }

        public void Heal(float amount) {
            if (isDead || currentHealth <= 0) return;
            currentHealth = Mathf.Min(currentHealth + amount, stats.maxHealth);
        }

        public void ShowSpeechBubble(string text, float duration = 4f)
        {
            Debug.Log($"Show speech bubble: {text}");
            if (speechBubblePrefab == null)
            {
                LogUtil.Warn("Unit", "ShowSpeechBubble", "SpeechBubble prefab not set");
                return;
            }

            if (currentBubble != null)
            {
                Destroy(currentBubble.gameObject);
            }

            Vector3 spawnPosition = speechBubbleAnchor != null
                ? speechBubbleAnchor.position
                : transform.position + Vector3.up * 2f;

            currentBubble = Instantiate(speechBubblePrefab, spawnPosition, Quaternion.identity);

            if (speechBubbleAnchor != null)
            {
                currentBubble.transform.SetParent(speechBubbleAnchor, worldPositionStays: true);
            }
            
            currentBubble.Show(text, duration);
        }

        public void SetCombatMode(AttackType attackType)
        {
            Debug.Log($"{name} set new combat mode {attackType}");
            if (currentAttackType == attackType)
            {
                Debug.Log($"{name} current attack type same to new, skipping");
                return;
            }

            currentAttackType = attackType;
            isInCombat = attackType != AttackType.None;
            
            if (animator != null)
            {
                if (hasCombatIdle)
                {
                    animator.SetBool(combatIdleHash, isInCombat);
                }

                if (hasAttackType)
                {
                    animator.SetFloat(attackTypeHash, (float) attackType);
                }
            }
            
            Debug.Log($"SetCombatMode: {name} set {attackType} mode. IsInCombat: {isInCombat}");
            UpdateSelectionVisual();
        }
        
        public void UpdateSelectionVisual()
        {
            if (!selectionVisual) {
                return;
            }

            if (isDead)
            {
                selectionVisual.Hide();
                return;
            }

            if (isSelected)
            {
                Color baseColor = UnitFactionColors.GetSelectionColor(faction);
                Color brightColor = Color.Lerp(baseColor, Color.white, 0.5f);
                selectionVisual.Show(transform, brightColor);
            }
            else if (IsPlayerControlled || isInCombat)
            {
                Color defaultColor = UnitFactionColors.GetSelectionColor(faction);
                selectionVisual.Show(transform, defaultColor);
            }
            else
            {
                selectionVisual.Hide();
            }
        }

        public void PlayAttackAnimation()
        {
            if (animator != null && hasAttackTrigger)
            {
                animator.SetTrigger(attackTriggerHash);
            }
        }

        public void RotateTowards(Vector3 position)
        {
            Vector3 direction = position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * stats.turnSpeed);
        }

        private void Die(Vector3? impactDirection = null) {
            if (isDead) return;
            isDead = true;

            if (TryGetComponent<Combat.UnitCombat>(out var combat))
            {
                combat.StopCombat();
            }

            if (isSelected) SetSelected(false);

            if (agent != null) {
                agent.isStopped = true;
                agent.updatePosition = false;
                agent.updateRotation = false;
                agent.enabled = false;
            }

            if (rootCollider != null) {
                rootCollider.enabled = false;
            }

            StartCoroutine(DeathSequence(impactDirection));
            Died?.Invoke(this);
        }

        private void ApplyStatsToAgent() {
            if (agent == null || !agent.isOnNavMesh) return;

            agent.acceleration = stats.acceleration;
            agent.stoppingDistance = stats.stoppingDistance;
        }

        private void UpdateMovementSpeed() {
            if (!agent || !agent.isOnNavMesh || agent.isStopped) {
                return;
            }

            bool shouldRun = false;

            switch (movementMode) {
                case MovementMode.ForcedWalk: {
                    shouldRun = false;
                    break;
                }
                case MovementMode.ForcedRun: {
                    shouldRun = true;
                    break;
                }
                case MovementMode.Auto:
                default: {
                    float remainingDistance = agent.remainingDistance;
                    if (float.IsPositiveInfinity(remainingDistance)) {
                        shouldRun = true;
                    } else {
                        shouldRun = remainingDistance > stats.runDistanceThreshold;
                    }
                    break;
                }
            }

            float targetSpeed = shouldRun ? stats.runSpeed : stats.walkSpeed;
            if (Mathf.Abs(agent.speed - targetSpeed) > 0.01f) {
                agent.speed = targetSpeed;
            }

            isRunning = shouldRun;
        }

        private void RotateTowardsMovementDirection() {
            if (agent == null || !agent.isOnNavMesh || agent.isStopped) return;

            Vector3 direction = agent.desiredVelocity;
            if (direction.sqrMagnitude < 0.001f) {
                direction = agent.velocity;
            }

            if (direction.sqrMagnitude < 0.001f) {
                return;
            }

            direction.y = 0f;

            if (direction.sqrMagnitude < 0.0001f) {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * stats.turnSpeed);
        }

        private void SetupAnimator() {
            if (animator == null) {
                animator = GetComponentInChildren<Animator>(true);
            }

            if (animator == null) return;

            if (disableRootMotion) {
                animator.applyRootMotion = false;
            }

            if (animator.runtimeAnimatorController == null) {
                return;
            }

            if (string.IsNullOrWhiteSpace(speedParameter)) {
                return;
            }

            speedParameterHash = Animator.StringToHash(speedParameter);

            foreach (AnimatorControllerParameter parameter in animator.parameters) {
                if (parameter.type == AnimatorControllerParameterType.Float && parameter.name == speedParameter) {
                    hasSpeedParameter = true;
                    break;
                }
            }

            if (!string.IsNullOrWhiteSpace(deathTrigger)) {
                deathTriggerHash = Animator.StringToHash(deathTrigger);
                foreach (var param in animator.parameters) {
                    if (param.type == AnimatorControllerParameterType.Trigger && param.name == deathTrigger) {
                        hasDeathTrigger = true;
                        break;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(attackTrigger))
            {
                attackTriggerHash = Animator.StringToHash(attackTrigger);
                foreach (var param in animator.parameters)
                {
                    if (param.type == AnimatorControllerParameterType.Trigger && param.name == attackTrigger)
                    {
                        hasAttackTrigger = true;
                        break;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(combatIdleTrigger))
            {
                combatIdleHash = Animator.StringToHash(combatIdleTrigger);
                foreach (var param in animator.parameters)
                {
                    if (param.type == AnimatorControllerParameterType.Bool && param.name == combatIdleTrigger)
                    {
                        hasCombatIdle = true;
                        break;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(attackTypeParam))
            {
                attackTypeHash = Animator.StringToHash(attackTypeParam);
                foreach (var param in animator.parameters)
                {
                    if (param.type == AnimatorControllerParameterType.Float && param.name == attackTypeParam)
                    {
                        hasAttackType = true;
                        break;
                    }
                }
            }
        }

        private void UpdateAnimation() {
            if (!animator || !hasSpeedParameter) {
                return;
            }

            float speedValue = 0f;

            if (agent && agent.isOnNavMesh && !agent.isStopped) {
                speedValue = agent.velocity.magnitude;
            }

            animator.SetFloat(speedParameterHash, speedValue);
        }

        private System.Collections.IEnumerator DeathSequence(Vector3? impactDirection) {
            if (animator != null && hasDeathTrigger) {
                animator.SetTrigger(deathTriggerHash);
            }

            yield return new WaitForSeconds(stats.deathAnimationDuration);

            if (ragdoll != null) {
                LogUtil.Info("Unit", "DeathSequence", "Ragdoll on");
                ragdoll.Activate(impactDirection);
            } else if (animator != null) {
                LogUtil.Info("Unit", "DeathSequence", "Ragdoll off");
                animator.enabled = false;
            }
        }
    }
}