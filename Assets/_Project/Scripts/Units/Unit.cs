using System;
using _Project.Scripts.Combat;
using _Project.Scripts.Units.Factions;
using _Project.Scripts.Utils;
using UnityEngine;

namespace _Project.Scripts.Units {

    /// <summary>
    /// Фасад юнита: агрегирует профильные компоненты и предоставляет единый API для
    /// внешних систем (состояния, AI, консоль, контроллеры ввода).
    ///
    /// SOLID:
    /// - SRP: Unit больше не реализует движение, анимацию, здоровье, смерть, выделение и пузыри речи —
    ///   за каждую ответственность отвечает отдельный компонент
    ///   (UnitMovement, UnitAnimatorController, UnitHealth, UnitDeathHandler,
    ///    UnitSelectionVisualController, UnitSpeechBubblePresenter).
    /// - OCP: новые реакции на смерть/выделение/бой добавляются подпиской на события Unit,
    ///   без изменения кода самого Unit.
    /// - LSP: все компоненты — самодостаточные MonoBehaviour-и, взаимозаменяемы по контракту событий.
    /// - ISP: потребители зависят только от нужных им членов (например, консоль — от TakeDamage/SaySomething),
    ///   а не от «тучного» класса с методами анимации, физики и UI в одном флаконе.
    /// - DIP: Unit зависит от абстракций (компонентов-соседей), а не от NavMeshAgent/Animator напрямую.
    /// </summary>
    [RequireComponent(typeof(UnitMovement))]
    [RequireComponent(typeof(UnitHealth))]
    [RequireComponent(typeof(UnitAnimatorController))]
    public class Unit : MonoBehaviour {

        [Header("Faction")] [SerializeField] private UnitFaction faction = UnitFaction.User;
        [Header("Stats")] [SerializeField] private UnitStats stats = new UnitStats();

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
        public float CurrentHealth => health != null ? health.CurrentHealth : 0f;
        public float MaxHealth => health != null ? health.MaxHealth : 0f;
        public bool IsInCombat => isInCombat;

        /// <summary>Направление последнего удара — читается UnitDeathHandler при смерти.</summary>
        public Vector3? LastImpactDirection => lastImpactDirection;

        public event Action<Unit> Destroyed;
        public event Action<Unit> Died;
        public event Action SelectionChanged;
        public event Action FactionChanged;
        public event Action CombatModeChanged;

        private void Awake() {
            movement = GetComponent<UnitMovement>();
            health = GetComponent<UnitHealth>();
            animatorController = GetComponent<UnitAnimatorController>();
            speechBubblePresenter = GetComponent<UnitSpeechBubblePresenter>();

            if (movement == null) {
                Debug.LogError($"{nameof(Unit)} requires {nameof(UnitMovement)} on the same GameObject", this);
            }

            if (health == null) {
                Debug.LogError($"{nameof(Unit)} requires {nameof(UnitHealth)} on the same GameObject", this);
            }

            health.Initialize(stats.maxHealth);
            movement.Initialize();
            animatorController.Initialize();
        }

        private void Update() {
            if (IsDead) return;

            if (!isInCombat) {
                movement.Tick();
            }

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

        // ---------------------------------------------------------------------
        // Выбор и фракция
        // ---------------------------------------------------------------------

        public void SetSelected(bool value) {
            if (isSelected == value) return;

            isSelected = value;
            SelectionChanged?.Invoke();
        }

        public void SetFaction(UnitFaction newFaction) {
            if (IsDead || faction == newFaction) return;

            faction = newFaction;
            FactionChanged?.Invoke();
        }

        // ---------------------------------------------------------------------
        // Движение (делегирование в UnitMovement)
        // ---------------------------------------------------------------------

        public bool MoveTo(Vector3 worldPosition) {
            return movement.MoveTo(worldPosition);
        }

        public void Stop() {
            movement.Stop();
        }

        public bool HasReachedDestination(float tolerance = 0.2f) {
            return movement.HasReachedDestination(tolerance);
        }

        public void SetMovementMode(MovementMode mode) {
            movement.SetMovementMode(mode);
        }

        // ---------------------------------------------------------------------
        // Бой и повороты
        // ---------------------------------------------------------------------

        public void SetCombatMode(AttackType attackType) {
            Debug.Log($"{name} set new combat mode {attackType}");

            if (currentAttackType == attackType) {
                Debug.Log($"{name} current attack type same to new, skipping");
                return;
            }

            currentAttackType = attackType;
            isInCombat = attackType != AttackType.None;

            animatorController.SetCombatState(isInCombat, attackType);

            Debug.Log($"SetCombatMode: {name} set {attackType} mode. IsInCombat: {isInCombat}");
            CombatModeChanged?.Invoke();
        }

        public void PlayAttackAnimation() {
            animatorController.PlayAttack();
        }

        public void RotateTowards(Vector3 position) {
            Vector3 direction = position - transform.position;
            RotateTowardsInternal(direction, Time.deltaTime * stats.turnSpeed);
        }

        // ---------------------------------------------------------------------
        // Здоровье (делегирование в UnitHealth)
        // ---------------------------------------------------------------------

        public void TakeDamage(float amount, Vector3? impactDirection = null) {
            if (amount <= 0f) return;

            lastImpactDirection = impactDirection;
            health.TakeDamage(amount, impactDirection);

            if (health.IsDead) {
                movement.SetDead(true);

                // Сначала уведомляем подписчиков (смерть, UI), затем сбрасываем выделение.
                Died?.Invoke(this);
                SetSelected(false);
            }
        }

        public void Heal(float amount) {
            if (amount <= 0f) return;

            health.Heal(amount);
        }

        // ---------------------------------------------------------------------
        // Речевой пузырь (делегирование в UnitSpeechBubblePresenter)
        // ---------------------------------------------------------------------

        public void SaySomething(string message) {
            LogUtil.Info("Unit", "SaySomething", message);
            ShowSpeechBubble(message);
        }

        public void ShowSpeechBubble(string text, float duration = 4f) {
            if (speechBubblePresenter == null) {
                LogUtil.Warn("Unit", "ShowSpeechBubble", "Speech bubble presenter is not attached");
                return;
            }

            speechBubblePresenter.Show(text, duration);
        }

        // ---------------------------------------------------------------------
        // Внутренняя логика ориентации и анимации
        // ---------------------------------------------------------------------

        private void RotateTowardsMovementDirection() {
            if (movement == null || !movement.IsMoving) return;

            Vector3 direction = movement.DesiredVelocity;
            if (direction.sqrMagnitude < 0.001f) {
                direction = movement.Velocity;
            }

            RotateTowardsInternal(direction, Time.deltaTime * stats.turnSpeed);
        }

        private void RotateTowardsInternal(Vector3 direction, float slerpT) {
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, slerpT);
        }

        private void UpdateAnimation() {
            if (animatorController == null) return;

            float speedValue = movement != null && movement.IsMoving ? movement.Velocity.magnitude : 0f;
            animatorController.SetSpeed(speedValue);
        }
    }
}
