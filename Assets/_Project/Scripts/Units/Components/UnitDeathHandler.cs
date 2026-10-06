using System.Collections;
using _Project.Scripts.Combat;
using _Project.Scripts.Utils;
using UnityEngine;

namespace _Project.Scripts.Units.Components
{
    /// <summary>
    /// Responsibility: unit behavior upon death—stopping combat, disabling the collider,
    /// playing the death animation, and activating ragdoll (SRP). 
    /// Subscribes to the Unit.Died event itself: Unit and UnitHealth know nothing about ragdolls or physics (DIP/OCP). 
    /// </summary>
    
    [DisallowMultipleComponent]
    public class UnitDeathHandler : MonoBehaviour
    {
        [SerializeField] private CapsuleCollider rootCollider;

        private Unit unit;
        private UnitMovement movement;
        private UnitAnimatorController animatorController;
        private UnitRagdoll ragdoll;

        private void Awake()
        {
            if (!TryGetComponent(out unit))
            {
                Debug.LogError($"{nameof(UnitDeathHandler)} requires {nameof(Unit)} on the GameObject", this);
            }

            movement = GetComponent<UnitMovement>();
            animatorController = GetComponent<UnitAnimatorController>();
            ragdoll = GetComponentInChildren<UnitRagdoll>(true);

            if (rootCollider == null)
            {
                rootCollider = GetComponent<CapsuleCollider>();
            }
        }

        private void OnEnable()
        {
            if (unit != null)
            {
                unit.Died += HandleDied;
            }
        }

        private void OnDisable()
        {
            if (unit != null)
            {
                unit.Died -= HandleDied;
            }
        }

        private void HandleDied()
        {
            StopCombat();
            DisableCollider();
            DisableMovement();

            StartCoroutine(DeathSequence(unit.LastImpactDirection));
        }

        private void StopCombat()
        {
            if (unit != null && unit.TryGetComponent<UnitCombat>(out var combat))
            {
                combat.StopCombat();
            }
        }

        private void DisableCollider()
        {
            if (rootCollider != null)
            {
                rootCollider.enabled = false;
            }
        }

        private void DisableMovement()
        {
            if (movement != null)
            {
                movement.Shutdown();
            }
        }

        private IEnumerator DeathSequence(Vector3? impactDirection)
        {
            if (animatorController != null)
            {
                animatorController.PlayDeath();
            }

            float duration = unit != null ? unit.Stats.deathAnimationDuration : 0f;
            yield return new WaitForSeconds(duration);

            if (ragdoll != null)
            {
                LogUtil.Info(nameof(UnitDeathHandler), nameof(DeathSequence), "Ragdoll on");
                ragdoll.Activate(impactDirection);
            } else if (animatorController != null)
            {
                LogUtil.Info(nameof(UnitDeathHandler), nameof(DeathSequence), "Ragdoll off");
                animatorController.Disable();
            }
        }
    }
}