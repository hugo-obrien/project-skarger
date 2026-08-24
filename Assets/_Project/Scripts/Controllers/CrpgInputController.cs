using _Project.Scripts.Units;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Scripts {
    public class CrpgInputController : MonoBehaviour {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private LayerMask unitLayer = 1;
        [SerializeField] private LayerMask groundLayer = 1;
        [SerializeField] private MoveMarkerVisual moveMarkerPrefab;
        [SerializeField] private Color moveMarkerColor = Color.darkGreen;
        [SerializeField] private bool deselectOnEmptyLeftClick = false;

        [SerializeField] [Min(1f)] private float maxRaycastDistance = 1000f;
        [SerializeField] [Min(0f)] private float moveMarkerOffset = 0.03f;

        private Unit selectedUnit;
        private MoveMarkerVisual currentMoveMarker;

        private void Reset() {
            mainCamera = Camera.main;
        }

        private void Update() {
            if (mainCamera == null) {
                mainCamera = Camera.main;
                if (mainCamera == null) {
                    return;
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape)) {
                Deselect();
                return;
            }

            if (IsPointerOverUI()) {
                return;
            }

            if (Input.GetMouseButtonDown(0)) {
                HandleSelectionClick();
            }

            if (Input.GetMouseButtonDown(1) && selectedUnit != null) {
                HandleMoveClick();
            }

            HideMoveMarkerIfReached();
        }

        private void HandleMoveClick() {
            if (selectedUnit == null || !selectedUnit.IsPlayerControlled) {
                return;
            }

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, maxRaycastDistance, groundLayer, QueryTriggerInteraction.Ignore)) {
                return;
            }

            if (!selectedUnit.MoveTo(hit.point)) {
                return;
            }

            SpawnMoveMarker(hit.point, hit.normal);
        }

        private void HandleSelectionClick() {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, maxRaycastDistance, unitLayer, QueryTriggerInteraction.Ignore)) {
                Unit unit = hit.collider.GetComponentInParent<Unit>();
                if (unit != null) {
                    Select(unit);
                    return;
                }
            }

            if (deselectOnEmptyLeftClick) {
                Deselect();
            }
        }

        private void Select(Unit unit) {
            if (selectedUnit == unit) {
                return;
            }
            
            selectedUnit?.Deselect();
            ClearMoveMarker();

            selectedUnit = unit;
            selectedUnit.Select();
        }
        
        private void Deselect() {
            selectedUnit?.Deselect();
            selectedUnit = null;

            ClearMoveMarker();
        }
        
        private void SpawnMoveMarker(Vector3 point, Vector3 normal) {
            if (moveMarkerPrefab == null) {
                Debug.Log("CrpgInputController.SpawnMoveMarker(): moveMarkerPrefab is null");
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
            if (currentMoveMarker == null || selectedUnit == null) {
                return;
            }

            if (selectedUnit.HasReachedDestination()) {
                currentMoveMarker.FadeOutAndDestroy(0.2f);
                currentMoveMarker = null;
            }
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
    }
}
