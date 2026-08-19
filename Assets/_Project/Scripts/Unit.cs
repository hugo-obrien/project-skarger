using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace _Project.Scripts {
    [RequireComponent(typeof(NavMeshAgent))]
    public class Unit : MonoBehaviour {
        [SerializeField] private UnitStats stats = new UnitStats();
        [SerializeField] private UnitSelectionVisual selectionVisual;

        [Header("Selection")]
        [SerializeField]
        private Color selectedColor = Color.lawnGreen;

        [Header("Animation (optional)")]
        [SerializeField] private Animator animator;

        [SerializeField]
        private string speedParameter = "Speed";

        [SerializeField]
        private bool disableRootMotion = true;

        private NavMeshAgent agent;
        private bool isSelected;

        private int speedParameterHash;
        private bool hasSpeedParameter;

        public bool IsSelected => isSelected;
        public UnitStats Stats => stats;

        private void Awake() {
            agent = GetComponent<NavMeshAgent>();
            agent.updateRotation = false;

            SetupAnimator();
            ApplyStatsToAgent();
        }

        private void Update() {
            ApplyStatsToAgent();
            RotateTowardsMovementDirection();
            UpdateAnimation();
        }

        public void Select() {
            if (isSelected) return;

            isSelected = true;
            if (selectionVisual != null) {
                selectionVisual.Show(transform, selectedColor);
            }
        }

        public void Deselect() {
            if (!isSelected) return;

            isSelected = false;
            if (selectionVisual != null) {
                selectionVisual.Hide();
            }
        }

        public bool MoveTo(Vector3 worldPosition) {
            if (agent == null || !agent.isOnNavMesh) return false;

            agent.isStopped = false;

            if (NavMesh.SamplePosition(worldPosition, out NavMeshHit navHit, stats.destinationSnapDistance, NavMesh.AllAreas)) {
                worldPosition = navHit.position;
            } else {
                return false;
            }

            return agent.SetDestination(worldPosition);
        }

        public void Stop() {
            if (agent == null || !agent.isOnNavMesh) return;

            agent.isStopped = true;
            agent.ResetPath();
        }

        public bool HasReachedDestination(float tolerance = 0.2f) {
            if (agent == null || !agent.isOnNavMesh) return true;
            
            if (agent.pathPending) return false;

            return agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, tolerance);
        }

        private void ApplyStatsToAgent() {
            if (agent == null || !agent.isOnNavMesh) return;

            agent.speed = stats.moveSpeed;
            agent.acceleration = stats.acceleration;
            agent.stoppingDistance = stats.stoppingDistance;
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
        }

        private void UpdateAnimation() {
            if (animator == null || !hasSpeedParameter) {
                return;
            }

            float speedValue = 0f;

            if (agent != null && agent.isOnNavMesh && !agent.isStopped) {
                speedValue = agent.velocity.magnitude;
            }
            
            animator.SetFloat(speedParameterHash, speedValue);
        }

    }
}
