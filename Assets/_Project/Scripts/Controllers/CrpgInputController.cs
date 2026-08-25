using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Markers;
using _Project.Scripts.Units;
using _Project.Scripts.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Scripts.Controllers {
    [RequireComponent(typeof(SelectionGroup))]
    public class CrpgInputController : MonoBehaviour {
        
        [Header("Camera")]
        [SerializeField] private Camera mainCamera;
        
        [Header("Layers")]
        [SerializeField] private LayerMask unitLayer = 1;
        [SerializeField] private LayerMask groundLayer = 1;
        
        [Header("Markers")]
        [SerializeField] private MoveMarkerVisual moveMarkerPrefab;
        [SerializeField] private Color moveMarkerColor = Color.darkGreen;
        [SerializeField] [Min(0f)] private float moveMarkerOffset = 0.03f;
        [SerializeField] private float moveMarkerFadeDuration = 0.25f;
        
        [Header("Selection")]
        [SerializeField] private bool deselectOnEmptyLeftClick = false;

        [Header("Group movement")] 
        [SerializeField] [Min(0.1f)] [Tooltip("Distance between units in group")]
        private float unitSpacing = 1.25f;
        [SerializeField] [Range(0.1f, 0.9f)] [Tooltip("Occupance check radius")]
        private float occupancyRadiusFactor = 0.45f;

        [SerializeField] [Min(1f)] private float maxRaycastDistance = 1000f;

        private SelectionGroup selectionGroup;
        
        private MoveMarkerVisual currentMoveMarker;

        private readonly List<Unit> activeMovers = new List<Unit>();

        private void Awake() {
            if (!TryGetComponent(out selectionGroup)) {
                LogUtil.Error("CrpgInputController", "Awake", "Requires SelectionGroup");
            }
        }

        private void Reset() {
            mainCamera = Camera.main;
        }

        private void Update() {
            
            if (mainCamera == null) {
                mainCamera = Camera.main;
                if (mainCamera == null) return;
            }

            if (Input.GetKeyDown(KeyCode.Escape)) {
                selectionGroup.Clear();
                return;
            }

            if (IsPointerOverUI()) {
                return;
            }

            if (Input.GetMouseButtonDown(0)) {
                HandleSelectionClick();
            }

            if (Input.GetMouseButtonDown(1) && selectionGroup.HasPlayerControlledUnits()) {
                HandleMoveClick();
            }

            HideMoveMarkerIfReached();
        }

        private void HandleMoveClick() {
            IReadOnlyList<Unit> playerUnits = selectionGroup.GetPlayerControlledUnits();
            if (playerUnits.Count == 0) return;

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, maxRaycastDistance, groundLayer, QueryTriggerInteraction.Ignore)) {
                return;
            }
            
            IReadOnlyList<Unit> movers = playerUnits
                .OrderBy(unit => Vector3.SqrMagnitude(unit.transform.position - hit.point))
                .ToList();

            activeMovers.Clear();
            ClearMoveMarker();

            float occupancyRadius = unitSpacing * occupancyRadiusFactor;
            IReadOnlyList<Vector3> destinations = 
                FormationResolver.BuildDestinations(movers, hit.point, unitSpacing, unitLayer, occupancyRadius);

            for (int i = 0; i < movers.Count; i++) {
                Unit mover = movers[i];
                if (mover == null) {
                    continue;
                }

                if (mover.MoveTo(destinations[i])) {
                    activeMovers.Add(mover);
                }
            }

            if (activeMovers.Count > 0) {
                SpawnMoveMarker(hit.point, hit.normal);
            }
        }

        private void HandleSelectionClick() {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (!Physics.Raycast(ray, out RaycastHit hit, maxRaycastDistance, unitLayer, QueryTriggerInteraction.Ignore)) {
                if (deselectOnEmptyLeftClick) {
                    selectionGroup.Clear();
                }
                return;
            }

            Unit unit = hit.collider.GetComponentInParent<Unit>();
            if (unit == null) {
                if (deselectOnEmptyLeftClick) {
                    selectionGroup.Clear();
                }
                return;
            }

            if (IsShiftPressed()) {
                if (unit.IsPlayerControlled && selectionGroup.HasPlayerControlledUnits()) {
                    selectionGroup.Toggle(unit);
                }
                return;
            }
            
            selectionGroup.SelectExclusive(unit);
        }
        
        private void SpawnMoveMarker(Vector3 point, Vector3 normal) {
            if (moveMarkerPrefab == null) {
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

        private void HideMoveMarkerIfReached() {
            if (currentMoveMarker == null) {
                activeMovers.Clear();
                return;
            }

            activeMovers.RemoveAll(unit => unit == null);
            if (activeMovers.Count == 0) {
                ClearMoveMarker();
                return;
            }

            foreach (var mover in activeMovers) {
                if (!mover.HasReachedDestination()) {
                    return;
                }
            }
            
            currentMoveMarker.FadeOutAndDestroy(moveMarkerFadeDuration);
            currentMoveMarker = null;
            activeMovers.Clear();
        }

        private void ClearMoveMarker() {
            if (currentMoveMarker == null) {
                return;
            }
            
            Destroy(currentMoveMarker.gameObject);
            currentMoveMarker = null;
        }

        private bool IsPointerOverUI() {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
        
        private bool IsShiftPressed() {
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }
    }
}
