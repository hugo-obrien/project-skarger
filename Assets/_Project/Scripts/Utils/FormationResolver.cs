using System.Collections.Generic;
using _Project.Scripts.Units;
using UnityEngine;
using UnityEngine.AI;

namespace _Project.Scripts.Utils {
    public static class FormationResolver {
        private const int AttemptsPerUnit = 24;
        private const float GoldenAngleRadians = 2.39996323f;
        private const float OccupancyCheckHeight = 1f;
        private const float ReservedDestinationSpacingMultiplier = 0.8f;

        public static IReadOnlyList<Vector3> BuildDestinations(
            IReadOnlyList<Unit> units,
            Vector3 target,
            float spacing,
            LayerMask unitLayer,
            float occupancyRadius
        ) {
            List<Vector3> destinations = new List<Vector3>(units.Count);
            for (int i = 0; i < units.Count; i++) {
                Vector3 destination = ResolveDestination(units[i], target, i, spacing, unitLayer, occupancyRadius,
                    destinations);
                destinations.Add(destination);
            }

            return destinations;
        }

        private static Vector3 ResolveDestination(Unit unit, Vector3 target, int unitIndex, float spacing,
            LayerMask unitLayer, float occupancyRadius, List<Vector3> reservedDestinations) {
            if (unitIndex == 0) {
                Vector3 exactDestination = SampleNavMesh(target, spacing);
                if (!IsBlocked(exactDestination, unit, unitLayer, occupancyRadius, reservedDestinations, spacing)) {
                    return exactDestination;
                }
            }

            for (int attempt = 0; attempt < AttemptsPerUnit; attempt++) {
                int pointIndex = Mathf.Max(1, unitIndex) + attempt * AttemptsPerUnit;
                Vector3 candidate = GetSpiralPoint(target, pointIndex, spacing);
                candidate = SampleNavMesh(candidate, spacing);

                if (!IsBlocked(candidate, unit, unitLayer, occupancyRadius, reservedDestinations, spacing)) {
                    return candidate;
                }
            }

            return SampleNavMesh(target, spacing);
        }

        private static Vector3 SampleNavMesh(Vector3 position, float maxDistance) {
            if (NavMesh.SamplePosition(position, out NavMeshHit hit, maxDistance, NavMesh.AllAreas))
                return hit.position;

            return position;
        }

        private static Vector3 GetSpiralPoint(Vector3 center, int index, float spacing) {
            float radius = spacing * Mathf.Sqrt(index);
            float angle = index * GoldenAngleRadians;

            return center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
        }

        private static bool IsBlocked(Vector3 position, Unit mover, LayerMask unitLayer, float occupancyRadius,
            IReadOnlyList<Vector3> reservedDestinations, float spacing) {
            if (IsTooCloseToReservedDestinations(position, reservedDestinations, spacing))
                return true;

            return IsOccupiedByOtherUnit(position, mover, unitLayer, occupancyRadius);
        }

        private static bool IsOccupiedByOtherUnit(Vector3 position, Unit mover, LayerMask unitLayer, float occupancyRadius) {
            Vector3 checkCenter = position + Vector3.up * OccupancyCheckHeight;
            Collider[] hits =
                Physics.OverlapSphere(checkCenter, occupancyRadius, unitLayer, QueryTriggerInteraction.Ignore);

            foreach (var hitCollider in hits) {
                Unit otherUnit = hitCollider.GetComponentInParent<Unit>();
                if (otherUnit != null && otherUnit != mover) {
                    return true;
                }
            }

            return false;
        }

        private static bool IsTooCloseToReservedDestinations(Vector3 position,
            IReadOnlyList<Vector3> reservedDestinations,
            float spacing) {
            float minDistance = spacing * ReservedDestinationSpacingMultiplier;
            float minDistanceSqr = minDistance * minDistance;

            for (int i = 0; i < reservedDestinations.Count; i++) {
                Vector3 delta = position - reservedDestinations[i];
                delta.y = 0f;

                if (delta.sqrMagnitude < minDistanceSqr) {
                    return true;
                }
            }

            return false;
        }
    }
}