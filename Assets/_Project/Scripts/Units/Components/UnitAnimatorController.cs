using System.Collections.Generic;
using _Project.Scripts.Combat;
using UnityEngine;

namespace _Project.Scripts.Units.Components
{
    /// <summary>
    /// Responsibility: solely the "unit ↔ Animator" link (SRP). 
    /// Encapsulates the lookup and setting of AnimatorController parameters,
    /// relieving the Unit of responsibility for animation details (DIP/LSP: the Unit depends on the
    /// "unit animation controller" abstraction rather than on knowledge of specific Animator parameters). 
    /// </summary>
    [DisallowMultipleComponent]
    public class UnitAnimatorController : MonoBehaviour
    {
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

        private readonly Dictionary<int, AnimatorControllerParameterType> parameterTypes =
            new Dictionary<int, AnimatorControllerParameterType>();

        private int speedHash;
        private bool hasSpeedParameter;

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>(true);
            }

            if (animator == null || animator.runtimeAnimatorController == null) return;
            if (hashes.Count > 0) return;

            if (disableRootMotion)
            {
                animator.applyRootMotion = false;
            }

            foreach (var parameter in animator.parameters)
            {
                knownParameterHashes.Add(parameter.nameHash);
                hashes[parameter.name] = parameter.nameHash;
                parameterTypes[parameter.nameHash] = parameter.type;
            }

            speedHash = ResolveHash(speedParameter, AnimatorControllerParameterType.Float, out hasSpeedParameter);
        }

        public void SetSpeed(float value)
        {
            if (animator == null || !hasSpeedParameter) return;
            
            animator.SetFloat(speedHash, value);
        }

        public void PlayAttack()
        {
            Trigger(attackTrigger);
        }

        public void SetCombatState(bool isInCombat, AttackType attackType)
        {
            if (animator == null) return;

            if (TryResolve(combatIdleTrigger, AnimatorControllerParameterType.Bool, out int idleHash))
            {
                animator.SetBool(idleHash, isInCombat);
            }

            if (TryResolve(attackTypeParam, AnimatorControllerParameterType.Float, out int typeHash))
            {
                animator.SetFloat(typeHash, (float) attackType);
            }
        }

        public void PlayDeath()
        {
            Trigger(deathTrigger);
        }

        public void Disable()
        {
            if (animator != null)
            {
                animator.enabled = false;
            }
        }

        private void Trigger(string parameterName)
        {
            if (animator == null) return;

            if (TryResolve(parameterName, AnimatorControllerParameterType.Trigger, out int hash))
            {
                animator.SetTrigger(hash);
            }
        }

        private int ResolveHash(string parameterName, AnimatorControllerParameterType exprectedType, out bool found)
        {
            found = TryResolve(parameterName, exprectedType, out int hash);
            return hash;
        }

        private bool TryResolve(string parameterName, AnimatorControllerParameterType exprectedType, out int hash)
        {
            hash = 0;

            if (string.IsNullOrWhiteSpace(parameterName) || animator == null) return false;

            int nameHash = Animator.StringToHash(parameterName);
            if (!knownParameterHashes.Contains(nameHash)) return false;

            if (parameterTypes.TryGetValue(nameHash, out AnimatorControllerParameterType type) && type != exprectedType)
            {
                return false;
            }

            return hashes.TryGetValue(parameterName, out hash);
        }
    }
}