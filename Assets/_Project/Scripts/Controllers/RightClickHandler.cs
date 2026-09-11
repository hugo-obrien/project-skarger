using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.UI;
using _Project.Scripts.Units;
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
        
        [Header("Group movement")] 
        [SerializeField] [Min(0.1f)] [Tooltip("Distance between units in group")]
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
        private MoveMarkerVisual currentMoveMarker;

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
                    LogUtil.Warn("RightClickHandler", "HandleRightClick","hit collider is null");
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
                    HandleUnitRightClick();
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

        private void HandleUnitRightClick()
        {
            LogUtil.Info("RightClickHandler", "HandleUnitRightClick", "Not implemented yet");
        }

        private void HandleMove(RaycastHit hit)
        {
            IReadOnlyList<Unit> units = selectionGroup.GetPlayerControlledUnits();
            if (units.Count == 0)
            {
                return;
            }

            activeMovers.Clear();
            ClearMoveMarker();

            float occupancyRadius = unitSpacing * occupancyRadiusFactor;
            IReadOnlyList<Vector3> destinations =
                FormationResolver.BuildDestinations(units, hit.point, unitSpacing, unitLayer, occupancyRadius);
            
            for (int i = 0; i < units.Count; i++) {
                Unit unit = units[i];
                if (unit == null) {
                    continue;
                }

                if (unit.MoveTo(destinations[i])) {
                    activeMovers.Add(unit);
                }
            }

            if (activeMovers.Count > 0) {
                SpawnMoveMarker(hit.point, hit.normal);
            }
        }
        
        private void ClearMoveMarker()
        {
            if (currentMoveMarker == null)
            {
                return;
            }

            Destroy(currentMoveMarker.gameObject);
            currentMoveMarker = null;
        }
        
        private void SpawnMoveMarker(Vector3 point, Vector3 normal)
        {
            if (moveMarkerPrefab == null)
            {
                LogUtil.Info("CrpgInputController", "SpawnMoveMarker", "moveMarkerPrefab is null");
                return;
            }

            ClearMoveMarker();

            Vector3 normalNormalized = normal.normalized;
            Vector3 markerPosition = point + normalNormalized * moveMarkerOffset;
            Quaternion markerRotation = Quaternion.FromToRotation(Vector3.up, normalNormalized);

            currentMoveMarker = Instantiate(moveMarkerPrefab, markerPosition, markerRotation);
            currentMoveMarker.Setup(markerPosition, markerRotation, moveMarkerColor);
        }
        
        private void HideMoveMarkerIfReached()
        {
            if (currentMoveMarker == null)
            {
                activeMovers.Clear();
                return;
            }

            activeMovers.RemoveAll(unit => unit == null);
            if (activeMovers.Count == 0)
            {
                ClearMoveMarker();
                return;
            }

            foreach (var mover in activeMovers)
            {
                if (!mover.HasReachedDestination())
                {
                    return;
                }
            }

            currentMoveMarker.FadeOutAndDestroy(moveMarkerFadeDuration);
            currentMoveMarker = null;
            activeMovers.Clear();
        }
        
    }
}