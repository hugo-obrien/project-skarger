using System;
using System.Collections;
using UnityEngine;
using _Project.Scripts.Units;

namespace _Project.Scripts.Combat {
    
    /// <summary>
    /// Компонент управления боем для юнита.
    /// Реализует RTWP (Real-Time With Pause) базовую механику:
    /// - Переключение между Combat/Non-Combat состояниями
    /// - Ближний бой с авто-подходом к цели
    /// - Дальний бой с позиционированием и стрельбой снарядами
    /// </summary>
    [RequireComponent(typeof(Unit))]
    public class UnitCombat : MonoBehaviour {
        
        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private Transform attackSpawnPoint; // точка спавна снаряда для ranged
        
        [Header("Animation Parameters")]
        [SerializeField] private string combatStateParameter = "InCombat";
        [SerializeField] private string attackTriggerParameter = "Attack";
        [SerializeField] private string idleCombatClipName = "IdleCombat";
        [SerializeField] private string punchLeftClipName = "PunchLeft";
        
        [Header("Combat Stats")]
        [SerializeField] private CombatStats combatStats = new CombatStats();
        
        [Header("Debug")]
        [SerializeField] private bool debugGizmos = true;
        
        private Unit unit;
        private NavMeshAgent agent;
        private CombatState currentCombatState = CombatState.None;
        
        private Unit currentTarget;
        private float timeSinceLastAttack;
        private bool isAttacking;
        private bool isRangedUnit => combatStats.rangedMaxRange > combatStats.meleeRange * 1.5f;
        
        private int combatStateHash;
        private int attackTriggerHash;
        private bool hasCombatAnimatorParams;
        
        // События
        public event Action<Unit, Unit> OnAttackStarted;
        public event Action<Unit, Unit> OnAttackHit;
        public event Action<Unit, CombatState> OnCombatStateChanged;
        
        // Публичные свойства
        public CombatState CurrentCombatState => currentCombatState;
        public Unit CurrentTarget => currentTarget;
        public bool IsInCombat => currentCombatState == CombatState.InCombat;
        public bool IsRangedUnit => isRangedUnit;
        
        private void Awake() {
            unit = GetComponent<Unit>();
            agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            
            SetupAnimator();
        }
        
        private void Update() {
            if (unit.IsDead) return;
            
            UpdateCombatState();
            UpdateAttackCooldown();
            
            if (currentCombatState == CombatState.InCombat && currentTarget != null) {
                HandleCombatBehavior();
            }
        }
        
        private void OnDestroy() {
            // Очистка при уничтожении
            if (currentTarget != null) {
                currentTarget.Died -= OnTargetDied;
            }
        }
        
        #region Public API
        
        /// <summary>
        /// Начать атаку на цель (вызывается из RightClickHandler при клике на врага)
        /// </summary>
        public void AttackTarget(Unit target) {
            if (target == null || target.IsDead) return;
            
            currentTarget = target;
            currentTarget.Died += OnTargetDied;
            
            EnterCombatState(CombatState.Alerted);
        }
        
        /// <summary>
        /// Принудительно выйти из боя
        /// </summary>
        public void ExitCombat() {
            EnterCombatState(CombatState.None);
            currentTarget = null;
        }
        
        /// <summary>
        /// Получить текущее время до следующей атаки
        /// </summary>
        public float GetTimeUntilNextAttack() {
            float attackSpeed = isRangedUnit ? combatStats.rangedAttackSpeed : combatStats.attackSpeed;
            float cooldown = isRangedUnit ? combatStats.rangedAttackCooldown : combatStats.attackCooldown;
            float attackInterval = 1f / attackSpeed + cooldown;
            return Mathf.Max(0f, attackInterval - timeSinceLastAttack);
        }
        
        #endregion
        
        #region Combat State Machine
        
        private void UpdateCombatState() {
            if (currentTarget == null) {
                if (currentCombatState != CombatState.None) {
                    EnterCombatState(CombatState.None);
                }
                return;
            }
            
            if (currentTarget.IsDead) {
                EnterCombatState(CombatState.None);
                currentTarget = null;
                return;
            }
            
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
            
            // Проверка на выход из аггро
            if (currentCombatState == CombatState.InCombat) {
                if (distanceToTarget > combatStats.deaggroRange) {
                    EnterCombatState(CombatState.None);
                    currentTarget = null;
                }
                return;
            }
            
            // Проверка на вход в аггро
            if (currentCombatState == CombatState.None) {
                if (distanceToTarget <= combatStats.aggroRange) {
                    EnterCombatState(CombatState.Alerted);
                }
            }
            // Переход из Alerted в InCombat
            else if (currentCombatState == CombatState.Alerted) {
                if (CanAttackTarget(distanceToTarget)) {
                    EnterCombatState(CombatState.InCombat);
                } else if (distanceToTarget > combatStats.chaseRange) {
                    EnterCombatState(CombatState.None);
                    currentTarget = null;
                }
            }
        }
        
        private void EnterCombatState(CombatState newState) {
            if (currentCombatState == newState) return;
            
            CombatState oldState = currentCombatState;
            currentCombatState = newState;
            
            // Обновляем аниматор
            UpdateAnimatorCombatState();
            
            OnCombatStateChanged?.Invoke(unit, newState);
            
            Debug.Log($"[UnitCombat] {gameObject.name}: Combat state changed from {oldState} to {newState}");
        }
        
        #endregion
        
        #region Combat Behavior
        
        private void HandleCombatBehavior() {
            if (currentTarget == null || currentTarget.IsDead) return;
            
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
            
            if (isRangedUnit) {
                HandleRangedBehavior(distanceToTarget);
            } else {
                HandleMeleeBehavior(distanceToTarget);
            }
            
            // Поворот к цели
            LookAtTarget();
            
            // Атака если возможно
            if (CanAttackTarget(distanceToTarget) && !isAttacking) {
                PerformAttack();
            }
        }
        
        private void HandleMeleeBehavior(float distanceToTarget) {
            // Если слишком далеко - подойти
            if (distanceToTarget > combatStats.meleeRange) {
                MoveToTarget(currentTarget.transform.position);
            } else {
                // В пределах досягаемости - остановиться и атаковать
                StopMovement();
            }
        }
        
        private void HandleRangedBehavior(float distanceToTarget) {
            float optimalRange = (combatStats.rangedMinRange + combatStats.rangedMaxRange) * 0.5f;
            
            // Если слишком близко - отойти
            if (distanceToTarget < combatStats.rangedMinRange) {
                Vector3 retreatPosition = transform.position - (currentTarget.transform.position - transform.position).normalized * combatStats.rangedMinRange;
                MoveToTarget(retreatPosition);
            }
            // Если слишком далеко - подойти
            else if (distanceToTarget > combatStats.rangedMaxRange) {
                MoveToTarget(currentTarget.transform.position);
            }
            // В оптимальной зоне - остановиться и стрелять
            else {
                StopMovement();
            }
        }
        
        private void MoveToTarget(Vector3 position) {
            if (agent != null && !unit.IsDead) {
                agent.isStopped = false;
                unit.MoveTo(position);
            }
        }
        
        private void StopMovement() {
            if (agent != null && !unit.IsDead) {
                agent.isStopped = true;
            }
        }
        
        private void LookAtTarget() {
            if (currentTarget == null) return;
            
            Vector3 directionToTarget = currentTarget.transform.position - transform.position;
            directionToTarget.y = 0;
            
            if (directionToTarget.sqrMagnitude > 0.001f) {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 
                    Time.deltaTime * unit.Stats.turnSpeed);
            }
        }
        
        #endregion
        
        #region Attack Logic
        
        private bool CanAttackTarget(float distanceToTarget) {
            if (currentTarget == null || currentTarget.IsDead) return false;
            
            if (isRangedUnit) {
                return distanceToTarget >= combatStats.rangedMinRange && 
                       distanceToTarget <= combatStats.rangedMaxRange;
            } else {
                return distanceToTarget <= combatStats.meleeRange;
            }
        }
        
        private void PerformAttack() {
            if (timeSinceLastAttack < GetAttackInterval()) return;
            
            isAttacking = true;
            timeSinceLastAttack = 0f;
            
            OnAttackStarted?.Invoke(unit, currentTarget);
            
            if (isRangedUnit) {
                PerformRangedAttack();
            } else {
                PerformMeleeAttack();
            }
        }
        
        private void PerformMeleeAttack() {
            // Запуск анимации атаки
            TriggerAttackAnimation();
            
            // Нанесение урона (можно сместить по времени через AnimationEvent)
            // Для простоты наносим урон сразу после небольшой задержки
            StartCoroutine(DelayedMeleeDamage());
        }
        
        private IEnumerator DelayedMeleeDamage() {
            // Ждём примерно половину анимации атаки (предполагаем ~0.3с)
            yield return new WaitForSeconds(0.3f);
            
            if (currentTarget != null && !currentTarget.IsDead) {
                currentTarget.TakeDamage(combatStats.meleeDamage, 
                    (currentTarget.transform.position - transform.position).normalized);
                OnAttackHit?.Invoke(unit, currentTarget);
            }
            
            isAttacking = false;
        }
        
        private void PerformRangedAttack() {
            // Запуск анимации атаки
            TriggerAttackAnimation();
            
            // Спавн снаряда
            SpawnProjectile();
            
            // Ranged атака считается совершённой сразу (урон нанесётся при попадании снаряда)
            isAttacking = false;
        }
        
        private void SpawnProjectile() {
            if (combatStats.ProjectilePrefab == null) {
                Debug.LogWarning($"[UnitCombat] {gameObject.name}: Projectile prefab not set!");
                return;
            }
            
            Transform spawnPoint = attackSpawnPoint != null ? attackSpawnPoint : 
                                   combatStats.ProjectileSpawnPoint != null ? combatStats.ProjectileSpawnPoint : 
                                   transform;
            
            Vector3 spawnPosition = spawnPoint.position;
            Vector3 directionToTarget = (currentTarget.transform.position - spawnPosition).normalized;
            
            // Добавляем небольшое упреждение для движущихся целей
            if (currentTarget.GetComponent<UnityEngine.AI.NavMeshAgent>() != null) {
                var targetAgent = currentTarget.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (targetAgent.velocity.sqrMagnitude > 0.01f) {
                    directionToTarget = (currentTarget.transform.position + targetAgent.velocity * 0.5f - spawnPosition).normalized;
                }
            }
            
            GameObject projectileObj = Instantiate(combatStats.ProjectilePrefab, spawnPosition, Quaternion.LookRotation(directionToTarget));
            
            CombatProjectile projectile = projectileObj.GetComponent<CombatProjectile>();
            if (projectile != null) {
                projectile.Initialize(spawnPosition, directionToTarget, 
                    combatStats.projectileSpeed, combatStats.projectileLifetime, combatStats.rangedDamage);
            } else {
                Debug.LogError($"[UnitCombat] {gameObject.name}: Projectile prefab missing CombatProjectile component!");
                Destroy(projectileObj);
            }
        }
        
        private float GetAttackInterval() {
            float baseSpeed = isRangedUnit ? combatStats.rangedAttackSpeed : combatStats.attackSpeed;
            float cooldown = isRangedUnit ? combatStats.rangedAttackCooldown : combatStats.attackCooldown;
            return 1f / baseSpeed + cooldown;
        }
        
        private void UpdateAttackCooldown() {
            if (!isAttacking) {
                timeSinceLastAttack += Time.deltaTime;
            }
        }
        
        #endregion
        
        #region Animation
        
        private void SetupAnimator() {
            if (animator == null) {
                animator = GetComponentInChildren<Animator>(true);
            }
            
            if (animator == null || animator.runtimeAnimatorController == null) {
                return;
            }
            
            combatStateHash = Animator.StringToHash(combatStateParameter);
            attackTriggerHash = Animator.StringToHash(attackTriggerParameter);
            
            // Проверяем наличие параметров
            bool hasCombatParam = false;
            bool hasAttackParam = false;
            
            foreach (var param in animator.parameters) {
                if (param.name == combatStateParameter && param.type == AnimatorControllerParameterType.Bool) {
                    hasCombatParam = true;
                }
                if (param.name == attackTriggerParameter && param.type == AnimatorControllerParameterType.Trigger) {
                    hasAttackParam = true;
                }
            }
            
            hasCombatAnimatorParams = hasCombatParam && hasAttackParam;
        }
        
        private void UpdateAnimatorCombatState() {
            if (!hasCombatAnimatorParams) return;
            
            animator.SetBool(combatStateHash, currentCombatState != CombatState.None);
        }
        
        private void TriggerAttackAnimation() {
            if (!hasCombatAnimatorParams) return;
            
            animator.SetTrigger(attackTriggerHash);
        }
        
        #endregion
        
        #region Callbacks
        
        private void OnTargetDied(Unit diedUnit) {
            if (diedUnit == currentTarget) {
                diedUnit.Died -= OnTargetDied;
                EnterCombatState(CombatState.None);
                currentTarget = null;
            }
        }
        
        #endregion
        
        #region Gizmos
        
        private void OnDrawGizmosSelected() {
            if (!debugGizmos) return;
            
            Color originalColor = Gizmos.color;
            
            // Aggro range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, combatStats.aggroRange);
            
            // Deaggro range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, combatStats.deaggroRange);
            
            // Melee range
            if (!isRangedUnit) {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(transform.position, combatStats.meleeRange);
            }
            
            // Ranged min/max range
            if (isRangedUnit) {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position, combatStats.rangedMinRange);
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.position, combatStats.rangedMaxRange);
            }
            
            // Current target line
            if (currentTarget != null) {
                Gizmos.color = currentTarget.IsDead ? Color.gray : Color.green;
                Gizmos.DrawLine(transform.position, currentTarget.transform.position);
            }
            
            Gizmos.color = originalColor;
        }
        
        #endregion
    }
}
