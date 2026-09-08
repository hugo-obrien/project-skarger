using System;
using System.Collections.Generic;
using _Project.Scripts.UI;
using _Project.Scripts.Units;
using _Project.Scripts.Utils;
using UnityEngine;
using UnityEngine.AI;

namespace _Project.Scripts.Controllers {
    
    /// <summary>
    /// Handles right-click context actions for selected units.
    /// Follows Single Responsibility Principle - only handles right-click target classification and actions.
    /// </summary>
    public class RightClickHandler {
        private readonly Camera mainCamera;
        private readonly LayerMask unitLayer;
        private readonly LayerMask groundLayer;
        private readonly float maxRaycastDistance;
        private readonly float unitSpacing;
        private readonly float occupancyRadiusFactor;
        
        private readonly DialogueBubble dialogueBubblePrefab;
        private readonly Color dialogueBubbleColor;
        private readonly MoveMarkerVisual moveMarkerPrefab;
        private readonly Color moveMarkerColor;
        private readonly float moveMarkerOffset;
        
        private readonly List<Unit> unitsInDialogueMode;
        private readonly List<Unit> unitsInCombatMode;
        private readonly Dictionary<Unit, float> attackCooldowns;
        
        private const float DialogueTriggerDistance = 3f;

        public RightClickHandler(
            Camera mainCamera,
            LayerMask unitLayer,
            LayerMask groundLayer,
            float maxRaycastDistance,
            float unitSpacing,
            float occupancyRadiusFactor,
            DialogueBubble dialogueBubblePrefab,
            Color dialogueBubbleColor,
            MoveMarkerVisual moveMarkerPrefab,
            Color moveMarkerColor,
            float moveMarkerOffset) {
            
            this.mainCamera = mainCamera;
            this.unitLayer = unitLayer;
            this.groundLayer = groundLayer;
            this.maxRaycastDistance = maxRaycastDistance;
            this.unitSpacing = unitSpacing;
            this.occupancyRadiusFactor = occupancyRadiusFactor;
            this.dialogueBubblePrefab = dialogueBubblePrefab;
            this.dialogueBubbleColor = dialogueBubbleColor;
            this.moveMarkerPrefab = moveMarkerPrefab;
            this.moveMarkerColor = moveMarkerColor;
            this.moveMarkerOffset = moveMarkerOffset;
            
            unitsInDialogueMode = new List<Unit>();
            unitsInCombatMode = new List<Unit>();
            attackCooldowns = new Dictionary<Unit, float>();
        }

        public void HandleRightClick(IReadOnlyList<Unit> playerUnits, Action<RightClickTarget> onTargetDetermined) {
            if (playerUnits.Count == 0) return;
            
            RightClickTarget target = DetermineRightClickTarget();
            onTargetDetermined?.Invoke(target);
            
            switch (target.Type) {
                case RightClickTargetType.Ground:
                    HandleGroundClick(playerUnits, target.Position);
                    break;
                case RightClickTargetType.FriendlyUnit:
                    HandleFriendlyUnitClick(playerUnits, target.TargetUnit);
                    break;
                case RightClickTargetType.NeutralOrAlliedUnit:
                    HandleNeutralOrAlliedUnitClick(playerUnits, target.TargetUnit);
                    break;
                case RightClickTargetType.EnemyUnit:
                    HandleEnemyUnitClick(playerUnits, target.TargetUnit);
                    break;
                default:
                    break;
            }
        }

        public void UpdateDialogueModeUnits() {
            if (unitsInDialogueMode.Count == 0) return;

            for (int i = unitsInDialogueMode.Count - 1; i >= 0; i--) {
                Unit unit = unitsInDialogueMode[i];
                if (unit == null || unit.IsDead) {
                    unitsInDialogueMode.RemoveAt(i);
                    continue;
                }

                if (!unit.HasReachedDestination()) continue;

                ShowDialogueBubble(unit, "Hello, Unit");
                unitsInDialogueMode.RemoveAt(i);
            }
        }

        public void UpdateCombatModeUnits() {
            if (unitsInCombatMode.Count == 0) return;

            for (int i = unitsInCombatMode.Count - 1; i >= 0; i--) {
                Unit unit = unitsInCombatMode[i];
                if (unit == null || unit.IsDead) {
                    unitsInCombatMode.RemoveAt(i);
                    continue;
                }

                Unit nearestEnemy = FindNearestEnemy(unit);
                if (nearestEnemy == null || nearestEnemy.IsDead) {
                    unitsInCombatMode.RemoveAt(i);
                    continue;
                }

                float distanceToEnemy = Vector3.Distance(unit.transform.position, nearestEnemy.transform.position);
                float combatDistance = nearestEnemy.Stats.combatDistance;

                if (distanceToEnemy <= combatDistance + 0.5f) {
                    PerformAttack(unit, nearestEnemy);
                } else if (!unit.HasReachedDestination()) {
                    continue;
                } else {
                    Vector3 approachPoint = CalculateApproachPoint(nearestEnemy.transform.position, combatDistance);
                    if (NavMesh.SamplePosition(approachPoint, out NavMeshHit navHit, unit.Stats.destinationSnapDistance, NavMesh.AllAreas)) {
                        unit.MoveTo(navHit.position);
                    }
                }
            }
        }

        public void ClearModeLists() {
            unitsInDialogueMode.Clear();
            unitsInCombatMode.Clear();
        }

        private RightClickTarget DetermineRightClickTarget() {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out RaycastHit unitHit, maxRaycastDistance, unitLayer, QueryTriggerInteraction.Ignore)) {
                Unit targetUnit = unitHit.collider.GetComponentInParent<Unit>();
                if (targetUnit != null && !targetUnit.IsDead) {
                    return ClassifyUnitTarget(targetUnit, unitHit.point);
                }
            }
            
            if (Physics.Raycast(ray, out RaycastHit groundHit, maxRaycastDistance, groundLayer, QueryTriggerInteraction.Ignore)) {
                return new RightClickTarget(RightClickTargetType.Ground, groundHit.point);
            }
            
            return new RightClickTarget(RightClickTargetType.None, Vector3.zero);
        }

        private RightClickTarget ClassifyUnitTarget(Unit targetUnit, Vector3 hitPoint) {
            switch (targetUnit.Faction) {
                case UnitFaction.User:
                    return new RightClickTarget(RightClickTargetType.FriendlyUnit, hitPoint, targetUnit);
                case UnitFaction.Allied:
                case UnitFaction.Neutral:
                    return new RightClickTarget(RightClickTargetType.NeutralOrAlliedUnit, hitPoint, targetUnit);
                case UnitFaction.Enemy:
                    return new RightClickTarget(RightClickTargetType.EnemyUnit, hitPoint, targetUnit);
                default:
                    return new RightClickTarget(RightClickTargetType.None, hitPoint, targetUnit);
            }
        }

        private void HandleGroundClick(IReadOnlyList<Unit> playerUnits, Vector3 position) {
            var result = CalculateMovement(playerUnits, position);
            ExecuteMovement(result.Movers, result.Destinations);
        }

        private void HandleFriendlyUnitClick(IReadOnlyList<Unit> playerUnits, Unit targetUnit) {
            Vector3 targetPosition = targetUnit.transform.position;
            var result = CalculateMovement(playerUnits, targetPosition);
            ExecuteMovement(result.Movers, result.Destinations);
        }

        private void HandleNeutralOrAlliedUnitClick(IReadOnlyList<Unit> playerUnits, Unit targetUnit) {
            unitsInDialogueMode.Clear();
            
            Vector3 targetPosition = targetUnit.transform.position;
            Vector3 approachPoint = CalculateApproachPoint(targetPosition, DialogueTriggerDistance);
            
            var result = CalculateMovement(playerUnits, approachPoint);
            ExecuteMovement(result.Movers, result.Destinations, unitsInDialogueMode);
        }

        private void HandleEnemyUnitClick(IReadOnlyList<Unit> playerUnits, Unit targetUnit) {
            unitsInCombatMode.Clear();
            
            float combatDistance = targetUnit.Stats.combatDistance;
            Vector3 targetPosition = targetUnit.transform.position;
            Vector3 approachPoint = CalculateApproachPoint(targetPosition, combatDistance);
            
            var result = CalculateMovement(playerUnits, approachPoint);
            ExecuteMovement(result.Movers, result.Destinations, unitsInCombatMode);
        }

        private struct MovementResult {
            public IReadOnlyList<Unit> Movers;
            public IReadOnlyList<Vector3> Destinations;
            
            public MovementResult(IReadOnlyList<Unit> movers, IReadOnlyList<Vector3> destinations) {
                Movers = movers;
                Destinations = destinations;
            }
        }

        private MovementResult CalculateMovement(IReadOnlyList<Unit> playerUnits, Vector3 targetPosition) {
            IReadOnlyList<Unit> movers = SortUnitsByDistance(playerUnits, targetPosition);
            float occupancyRadius = unitSpacing * occupancyRadiusFactor;
            IReadOnlyList<Vector3> destinations =
                FormationResolver.BuildDestinations(movers, targetPosition, unitSpacing, unitLayer, occupancyRadius);
            return new MovementResult(movers, destinations);
        }

        private void ExecuteMovement(IReadOnlyList<Unit> movers, IReadOnlyList<Vector3> destinations, List<Unit> modeList = null) {
            for (int i = 0; i < movers.Count; i++) {
                Unit mover = movers[i];
                if (mover == null) continue;

                if (mover.MoveTo(destinations[i])) {
                    modeList?.Add(mover);
                }
            }
        }

        private Vector3 CalculateApproachPoint(Vector3 targetPosition, float distance) {
            Vector3 averagePosition = GetAverageUnitPosition();
            Vector3 direction = (averagePosition - targetPosition).normalized;
            if (direction.sqrMagnitude < 0.001f) direction = Vector3.forward;
            return targetPosition + direction * distance;
        }

        private Vector3 GetAverageUnitPosition() {
            var playerUnits = UnitRegistry.Units;
            if (playerUnits.Count == 0) return Vector3.zero;

            Vector3 averagePosition = Vector3.zero;
            int count = 0;
            foreach (var unit in playerUnits) {
                if (unit != null && !unit.IsDead) {
                    averagePosition += unit.transform.position;
                    count++;
                }
            }
            return count > 0 ? averagePosition / count : Vector3.zero;
        }

        private IReadOnlyList<Unit> SortUnitsByDistance(IReadOnlyList<Unit> units, Vector3 target) {
            List<Unit> sorted = new List<Unit>(units);
            sorted.Sort((a, b) => {
                float distA = Vector3.SqrMagnitude(a.transform.position - target);
                float distB = Vector3.SqrMagnitude(b.transform.position - target);
                return distA.CompareTo(distB);
            });
            return sorted;
        }

        private Unit FindNearestEnemy(Unit attacker) {
            Unit nearestEnemy = null;
            float nearestDistanceSqr = float.MaxValue;

            foreach (var unit in UnitRegistry.Units) {
                if (unit == null || unit.IsDead || unit.Faction != UnitFaction.Enemy) continue;

                float distanceSqr = Vector3.SqrMagnitude(attacker.transform.position - unit.transform.position);
                if (distanceSqr < nearestDistanceSqr) {
                    nearestDistanceSqr = distanceSqr;
                    nearestEnemy = unit;
                }
            }

            return nearestEnemy;
        }

        private void PerformAttack(Unit attacker, Unit target) {
            if (!attackCooldowns.ContainsKey(attacker)) {
                attackCooldowns[attacker] = 0f;
            }

            attackCooldowns[attacker] -= Time.deltaTime;
            if (attackCooldowns[attacker] > 0f) return;

            float damage = attacker.Stats.damage;
            target.TakeDamage(damage, (target.transform.position - attacker.transform.position).normalized);
            
            attackCooldowns[attacker] = attacker.Stats.attackCooldown;
            
            LogUtil.Info("RightClickHandler", "PerformAttack", $"{attacker.name} attacks {target.name} for {damage} damage");
        }

        private void ShowDialogueBubble(Unit targetUnit, string message) {
            if (dialogueBubblePrefab == null) {
                LogUtil.Info("RightClickHandler", "ShowDialogueBubble", "dialogueBubblePrefab is null");
                return;
            }

            DialogueBubble bubble = UnityEngine.Object.Instantiate(dialogueBubblePrefab, targetUnit.transform.position, Quaternion.identity);
            bubble.Setup(targetUnit.transform.position, message, dialogueBubbleColor);
        }
    }
}
