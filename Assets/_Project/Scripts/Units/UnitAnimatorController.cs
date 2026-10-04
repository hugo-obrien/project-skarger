using System.Collections.Generic;
using _Project.Scripts.Combat;
using UnityEngine;

namespace _Project.Scripts.Units {

    /// <summary>
    /// Ответственность: только связка «юнит ↔ Animator» (SRP).
    /// Инкапсулирует поиск параметров в AnimatorController и их установку,
    /// снимая с Unit ответственность за детали анимации (DIP/LSP: Unit зависит от абстракции
    /// «анимационный контроллер юнита», а не от знаний о конкретных параметрах Animator).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UnitAnimatorController : MonoBehaviour {

        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private bool disableRootMotion = true;

        [Header("Parameters")]
        [SerializeField] private string speedParameter = "Speed";
        [SerializeField] private string deathTrigger = "Death";
        [SerializeField] private string attackTrigger = "Attack";
        [SerializeField] private string combatIdleTrigger = "IsInCombat";
        [SerializeField] private string attackTypeParam = "AttackType";

        private readonly Dictionary<string, int> hashes = new Dictionary<string, int>();
        private readonly HashSet<int> knownParameterHashes = new HashSet<int>();
        private readonly Dictionary<int, AnimatorControllerParameterType> parameterTypes = new Dictionary<int, AnimatorControllerParameterType>();

        private int speedHash;
        private bool hasSpeedParameter;

        private void Awake() {
            Initialize();
        }

        /// <summary>Ленивая инициализация — безопасно вызывать повторно после Awake.</summary>
        public void Initialize() {
            if (animator == null) {
                animator = GetComponentInChildren<Animator>(true);
            }

            if (animator == null || animator.runtimeAnimatorController == null) return;
            if (hashes.Count > 0) return; // уже инициализирован

            if (disableRootMotion) {
                animator.applyRootMotion = false;
            }

            foreach (AnimatorControllerParameter parameter in animator.parameters) {
                knownParameterHashes.Add(parameter.nameHash);
                hashes[parameter.name] = parameter.nameHash;
                parameterTypes[parameter.nameHash] = parameter.type;
            }

            speedHash = ResolveHash(speedParameter, AnimatorControllerParameterType.Float, out hasSpeedParameter);
        }

        public void SetSpeed(float value) {
            if (animator == null || !hasSpeedParameter) return;

            animator.SetFloat(speedHash, value);
        }

        public void PlayAttack() {
            Trigger(attackTrigger);
        }

        public void SetCombatState(bool isInCombat, AttackType attackType) {
            if (animator == null) return;

            if (TryResolve(combatIdleTrigger, AnimatorControllerParameterType.Bool, out int idleHash)) {
                animator.SetBool(idleHash, isInCombat);
            }

            if (TryResolve(attackTypeParam, AnimatorControllerParameterType.Float, out int typeHash)) {
                animator.SetFloat(typeHash, (float) attackType);
            }
        }

        public void PlayDeath() {
            Trigger(deathTrigger);
        }

        public void Disable() {
            if (animator != null) {
                animator.enabled = false;
            }
        }

        private void Trigger(string parameterName) {
            if (animator == null) return;

            if (TryResolve(parameterName, AnimatorControllerParameterType.Trigger, out int hash)) {
                animator.SetTrigger(hash);
            }
        }

        private int ResolveHash(string parameterName, AnimatorControllerParameterType expectedType, out bool found) {
            found = TryResolve(parameterName, expectedType, out int hash);
            return hash;
        }

        private bool TryResolve(string parameterName, AnimatorControllerParameterType expectedType, out int hash) {
            hash = 0;

            if (string.IsNullOrWhiteSpace(parameterName) || animator == null) return false;

            int nameHash = Animator.StringToHash(parameterName);
            if (!knownParameterHashes.Contains(nameHash)) return false;

            // Тип параметра закэширован при инициализации — повторного обхода animator.parameters нет.
            if (parameterTypes.TryGetValue(nameHash, out AnimatorControllerParameterType type) && type != expectedType) {
                return false;
            }

            return hashes.TryGetValue(parameterName, out hash);
        }
    }
}
