using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Combat;
using _Project.Scripts.UI;
using _Project.Scripts.Units;
using _Project.Scripts.Units.Factions;
using _Project.Scripts.Utils;
using UnityEngine;

namespace _Project.Scripts.Controllers
{
    public enum InteractiveLayerType
    {
        Ground,
        Unit
    }

    [Serializable]
    public sealed class InteractiveLayerEntry
    {
        public InteractiveLayerType type;
        public LayerMask layerMask;
    }

    [RequireComponent(typeof(SelectionGroup))]
    public class RightClickHandler : MonoBehaviour
    {
        [Header("Camera")] [SerializeField] private Camera camera;

        [SerializeField] private InteractiveLayerEntry[] interactiveLayers;
        [SerializeField] [Min(1f)] private float maxRaycastDistance = 1000f;

        [Header("Group movement")] [SerializeField] [Min(0.1f)] [Tooltip("Distance between units in group")]
        private float unitSpacing = 1.25f;

        [SerializeField] [Range(0.1f, 0.9f)] [Tooltip("Occupance check radius")]
        private float occupancyRadiusFactor = 0.45f;

        [Header("Markers")] [SerializeField] private MoveMarkerVisual moveMarkerPrefab;
        [SerializeField] private Color moveMarkerColor = Color.darkGreen;
        [SerializeField] [Min(0f)] private float moveMarkerOffset = 0.03f;
        [SerializeField] private float moveMarkerFadeDuration = 0.25f;

        private SelectionGroup selectionGroup;

        private int combinedClickedMask;

        private readonly List<Unit> activeMovers = new();
        private LayerMask unitLayer;
        private readonly List<MoveMarkerVisual> activeMoveMarkers = new List<MoveMarkerVisual>();

        private Unit currentInteractionTarget;

        private void Awake()
        {
            if (!TryGetComponent(out selectionGroup))
            {
                LogUtil.Error("RightClickHandler", "Awake", "Selection group not found");
            }

            interactiveLayers.ToDictionary(entry => entry.layerMask.value, entry => entry);
            foreach (var mask in interactiveLayers)
            {
                combinedClickedMask |= mask.layerMask;
                if (mask.type == InteractiveLayerType.Unit)
                {
                    unitLayer = mask.layerMask;
                }
            }
        }

        private void Update()
        {
            if (camera == null)
            {
                camera = Camera.main;
                if (camera == null)
                {
                    LogUtil.Error("RightClickHandler", "Update", "Camera not found");
                    return;
                }
            }

            if (Input.GetMouseButtonDown(1) && !UiUtils.IsPointerOverUI() && selectionGroup.HasPlayerControlledUnits())
            {
                HandleRightClick();
            }

            HideMoveMarkerIfReached();
        }

        private void HandleRightClick()
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);

            RaycastHit[] hits = Physics.RaycastAll(ray, maxRaycastDistance, combinedClickedMask,
                QueryTriggerInteraction.Ignore);
            if (hits.Length == 0)
            {
                return;
            }

            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (var hit in hits)
            {
                if (hit.collider == null)
                {
                    LogUtil.Warn("RightClickHandler", "HandleRightClick", "hit collider is null");
                    continue;
                }

                int layerIndex = hit.collider.gameObject.layer;
                int layerBit = 1 << layerIndex;
                foreach (var entry in interactiveLayers)
                {
                    if ((entry.layerMask.value & layerBit) != 0)
                    {
                        HandleRightClickByLayer(entry.type, hit);
                        return;
                    }
                }
            }
        }

        private void HandleRightClickByLayer(InteractiveLayerType layer, RaycastHit hit)
        {
            switch (layer)
            {
                case InteractiveLayerType.Ground:
                {
                    HandleMove(hit);
                    break;
                }
                case InteractiveLayerType.Unit:
                {
                    HandleUnitRightClick(hit);
                    break;
                }
                default:
                {
                    LogUtil.Info("RightClickHandler", "HandleRightClickByLayer",
                        $"Behavior for {layer} not implemented yet");
                    break;
                }
            }
        }

        private void HandleUnitRightClick(RaycastHit hit)
        {
            Unit unit = hit.collider.GetComponentInParent<Unit>();
            if (unit == null)
            {
                LogUtil.Warn("RightClickHandler", "HandleUnitRightClick",
                    "Hit collider is on unit layer but has no Unit component");
                return;
            }

            if (unit.IsDead) return;
            
            HandleUnitRightClick(unit);
        }

        private void HandleUnitRightClick(Unit unit)
        {
            switch (unit.Faction)
            {
                case UnitFaction.User:
                {
                    HandleMove(unit.transform.position);
                    return;
                }
                case UnitFaction.Allied:
                case UnitFaction.Neutral:
                {
                    HandleAlliedOrNeutralClick(unit);
                    return;
                }
                case UnitFaction.Enemy:
                {
                    HandleAttack(unit);
                    return;
                }
                default:
                {
                    LogUtil.Warn("RightClickHandler", "HandleUnitRightClick", 
                        $"Unknown unit faction {unit.Faction}");
                    return;
                }
            }
        }

        private void HandleAttack(Unit target)
        {
            IReadOnlyList<Unit> units = selectionGroup.GetPlayerControlledUnits();
            if (units.Count == 0) return;
            
            ClearMoveMarkers();
            currentInteractionTarget = null;
            activeMovers.Clear();

            foreach (var unit in units)
            {
                if (unit == null) continue;
                if (unit.TryGetComponent<Combat.UnitCombat>(out var combat))
                {
                    combat.Attack(target);
                }
            }
        }

        private void HandleAlliedOrNeutralClick(Unit targetUnit)
        {
            IReadOnlyList<Unit> units = selectionGroup.GetPlayerControlledUnits();
            if (units.Count == 0) return;
            
            activeMovers.Clear();
            currentInteractionTarget = targetUnit;
            ClearMoveMarkers();

            foreach (var unit in units)
            {
                if (unit == null) continue;
                if (unit.MoveTo(targetUnit.transform.position))
                {
                    activeMovers.Add(unit);
                }
            }
        }

        private void HandleMove(RaycastHit hit)
        {
            IReadOnlyList<Unit> units = selectionGroup.GetPlayerControlledUnits();
            if (units.Count == 0) return;
            
            IssueMoveCommand(units, hit.point, hit.normal);
        }

        private void HandleMove(Vector3 point)
        {
            IReadOnlyList<Unit> units = selectionGroup.GetPlayerControlledUnits();
            if (units.Count == 0) return;
            
            IssueMoveCommand(units, point, null);
        }

        private void IssueMoveCommand(IReadOnlyList<Unit> units, Vector3 point, Vector3? normal = null)
        {
            activeMovers.Clear();
            ClearMoveMarkers();
            currentInteractionTarget = null;
            
            float occupancyRadius = unitSpacing * occupancyRadiusFactor;
            IReadOnlyList<Vector3> destinations =
                FormationResolver.BuildDestinations(units, point, unitSpacing, unitLayer, occupancyRadius);

            for (int i = 0; i < units.Count; i++)
            {
                Unit unit = units[i];
                if (unit == null) continue;

                if (unit.TryGetComponent<UnitCombat>(out var combat))
                {
                    combat.StopCombat();
                }

                if (unit.MoveTo(destinations[i]))
                {
                    activeMovers.Add(unit);
                }
            }

            if (activeMovers.Count > 0 && normal.HasValue)
            {
                SpawnMoveMarkers(destinations, normal.Value);
            }
        }

        private void SpawnMoveMarkers(IReadOnlyList<Vector3> points, Vector3 normal)
        {
            if (moveMarkerPrefab == null)
            {
                LogUtil.Info("CrpgInputController", "SpawnMoveMarker", "moveMarkerPrefab is null");
                return;
            }

            ClearMoveMarkers();

            Vector3 normalNormalized = normal.normalized;
            Quaternion markerRotation = Quaternion.FromToRotation(Vector3.up, normalNormalized);

            foreach (var point in points)
            {
                Vector3 markerPosition = point + normalNormalized * moveMarkerOffset;
                var marker = Instantiate(moveMarkerPrefab, markerPosition, markerRotation);
                marker.Setup(point, markerRotation, moveMarkerColor);
                activeMoveMarkers.Add(marker);
            }
        }

        private void HideMoveMarkerIfReached()
        {
            if (activeMoveMarkers.Count == 0 && currentInteractionTarget == null)
            {
                activeMovers.Clear();
                return;
            }

            activeMovers.RemoveAll(unit => unit == null);
            if (activeMovers.Count == 0)
            {
                ClearMoveMarkers();
                currentInteractionTarget = null;
                return;
            }

            bool allReached = true;
            foreach (var mover in activeMovers)
            {
                if (!mover.HasReachedDestination())
                {
                    allReached = false;
                    break;
                }
            }

            if (!allReached) return;

            if (activeMoveMarkers.Count > 0)
            {
                foreach (var marker in activeMoveMarkers)
                {
                    if (marker != null)
                    {
                        marker.FadeOutAndDestroy(moveMarkerFadeDuration);
                    }
                }
                activeMoveMarkers.Clear();
            }

            if (currentInteractionTarget != null)
            {
                currentInteractionTarget.ShowSpeechBubble("Hello, traveller!");
                currentInteractionTarget = null;
            }
            
            activeMovers.Clear();
        }
        
        private void ClearMoveMarkers()
        {
            foreach (var marker in activeMoveMarkers)
            {
                if (marker != null)
                {
                    Destroy(marker.gameObject);
                }
                activeMoveMarkers.Clear();
            }
        }
    }
}