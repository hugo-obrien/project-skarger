using _Project.Scripts.Movement;
using _Project.Scripts.Units.Stats;
using UnityEngine;
using UnityEngine.AI;

namespace _Project.Scripts.Units.Components
{
    /// <summary>
    /// Responsibility: moving the unit via NavMeshAgent and selecting the speed (walk/run)
    /// based on MovementMode (SRP, DIP — the Unit does not interact with the agent directly). 
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class UnitMovement : MonoBehaviour
    {
        private const float DestinationSnapDistance = 0.05f;
        private const float SpeedEpsilon = 0.01f;

        private NavMeshAgent agent;
        private Unit unit;
        private UnitStats Stats => unit != null ? unit.Stats : null;

        private MovementMode movementMode = MovementMode.Auto;

        private bool isDead;

        public bool IsRunning { get; private set; }
        public MovementMode Mode => movementMode;

        public Vector3 DesiredVelocity => agent != null ? agent.desiredVelocity : Vector3.zero;
        public Vector3 Velocity => agent != null ? agent.velocity : Vector3.zero;
        public bool IsMoving => !CannotMove() && !agent.isStopped;

        public void SetDead(bool value)
        {
            isDead = value;
        }

        public void Initialize()
        {
            unit = GetComponent<Unit>();
            agent = GetComponent<NavMeshAgent>();
            agent.updateRotation = false;

            if (agent.isOnNavMesh && Stats != null)
            {
                agent.acceleration = Stats.acceleration;
                agent.stoppingDistance = Stats.stoppingDistance;
            }
        }

        public void SetMovementMode(MovementMode mode)
        {
            movementMode = mode;
        }

        public bool MoveTo(Vector3 worldPosition)
        {
            if (CannotMove()) return false;

            agent.stoppingDistance = DestinationSnapDistance;
            agent.isStopped = false;

            if (!NavMesh.SamplePosition(worldPosition, out NavMeshHit navHit, Stats.destinationSnapDistance, NavMesh.AllAreas))
            {
                return false;
            }

            return agent.SetDestination(navHit.position);
        }

        public void Stop()
        {
            if (CannotMove()) return;

            agent.isStopped = true;
            agent.ResetPath();
        }

        public bool HasReachedDestination(float tolerance = 0.2f)
        {
            if (CannotMove()) return true;
            if (agent.pathPending) return false;

            return agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, tolerance);
        }

        public void Tick()
        {
            if (isDead || !IsMoving) return;

            bool shouldRun = ShouldRun();
            float targetSpeed = shouldRun ? Stats.runSpeed : Stats.walkSpeed;

            if (Mathf.Abs(agent.speed - targetSpeed) > SpeedEpsilon)
            {
                agent.speed = targetSpeed;
            }

            IsRunning = shouldRun;
        }

        public void Shutdown()
        {
            if (agent == null) return;

            agent.isStopped = true;
            agent.updatePosition = false;
            agent.updateRotation = false;
            agent.enabled = false;
        }

        private bool CannotMove()
        {
            return isDead || agent == null || !agent.isOnNavMesh;
        }

        private bool ShouldRun()
        {
            switch (movementMode)
            {
                case MovementMode.ForcedWalk: return false;
                case MovementMode.ForcedRun: return true;
                case MovementMode.Auto:
                default:
                {
                    float remainingDistance = agent.remainingDistance;
                    if (float.IsPositiveInfinity(remainingDistance)) return true;

                    return remainingDistance > Stats.runDistanceThreshold;
                }
            }
        }
    }
}