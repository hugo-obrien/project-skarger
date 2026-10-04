using UnityEngine;

namespace _Project.Scripts.Units {

    /// <summary>
    /// Ответственность: движение юнита через NavMeshAgent и выбор скорости (ходьба/бег)
    /// в зависимости от MovementMode (SRP, DIP — Unit не трогает агент напрямую).
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class UnitMovement : MonoBehaviour {

        private const float DestinationSnapDistance = 0.05f;
        private const float SpeedEpsilon = 0.01f;

        private NavMeshAgent agent;
        private Unit unit;
        private UnitStats Stats => unit != null ? unit.Stats : null;
        private bool isDead;
        private MovementMode movementMode = MovementMode.Auto;

        public bool IsRunning { get; private set; }
        public MovementMode Mode => movementMode;

        /// <summary>Вызывается из Unit при переходе юнита в мёртвое состояние.</summary>
        public void SetDead(bool value) {
            isDead = value;
        }

        /// <summary>
        /// Вызывается из Unit.Awake. Статы читаются с соседнего Unit напрямую —
        /// порядок инициализации компонентов не важен.
        /// </summary>
        public void Initialize() {
            unit = GetComponent<Unit>();
            agent = GetComponent<NavMeshAgent>();
            agent.updateRotation = false;

            if (agent.isOnNavMesh && Stats != null) {
                agent.acceleration = Stats.acceleration;
                agent.stoppingDistance = Stats.stoppingDistance;
            }
        }

        public void SetMovementMode(MovementMode mode) {
            movementMode = mode;
        }

        public bool MoveTo(Vector3 worldPosition) {
            if (isDead || !agent || !agent.isOnNavMesh) return false;

            agent.stoppingDistance = DestinationSnapDistance;
            agent.isStopped = false;

            if (!NavMesh.SamplePosition(worldPosition, out NavMeshHit navHit, Stats.destinationSnapDistance, NavMesh.AllAreas)) {
                return false;
            }

            return agent.SetDestination(navHit.position);
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

        public Vector3 DesiredVelocity => agent != null ? agent.desiredVelocity : Vector3.zero;
        public Vector3 Velocity => agent != null ? agent.velocity : Vector3.zero;
        public bool IsMoving => !isDead && agent != null && agent.isOnNavMesh && !agent.isStopped;

        /// <summary>Кадровое обновление режима ходьбы/бега. Вызывается из Unit.</summary>
        public void Tick() {
            if (isDead || !IsMoving) return;

            bool shouldRun = ShouldRun();
            float targetSpeed = shouldRun ? Stats.runSpeed : Stats.walkSpeed;

            if (Mathf.Abs(agent.speed - targetSpeed) > SpeedEpsilon) {
                agent.speed = targetSpeed;
            }

            IsRunning = shouldRun;
        }

        /// <summary>Останавливает юнит при смерти (отключает агента).</summary>
        public void Shutdown() {
            if (agent == null) return;

            agent.isStopped = true;
            agent.updatePosition = false;
            agent.updateRotation = false;
            agent.enabled = false;
        }

        private bool ShouldRun() {
            switch (movementMode) {
                case MovementMode.ForcedWalk:
                    return false;
                case MovementMode.ForcedRun:
                    return true;
                case MovementMode.Auto:
                default: {
                    float remainingDistance = agent.remainingDistance;
                    if (float.IsPositiveInfinity(remainingDistance)) return true;
                    return remainingDistance > Stats.runDistanceThreshold;
                }
            }
        }
    }
}
