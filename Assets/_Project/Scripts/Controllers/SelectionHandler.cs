using System.Collections.Generic;
using _Project.Scripts.UI;
using _Project.Scripts.Units;
using UnityEngine;

namespace _Project.Scripts.Controllers {
    public class SelectionHandler {
        public Camera MainCamera { get; set; }

        private readonly SelectionGroup selectionGroup;
        private readonly SelectionBoxOverlay selectionBoxOverlay;
        private readonly float dragThresholdPixels;
        private readonly LayerMask unitLayer;
        private readonly float maxRaycastDistance;

        private bool isLeftPointerDown;
        private bool isBoxSelecting;
        private Vector2 boxStartScreenPosition;

        public SelectionHandler(SelectionGroup selectionGroup, SelectionBoxOverlay selectionBoxOverlay,
            float dragThresholdPixels, LayerMask unitLayer, float maxRaycastDistance) {
            this.selectionGroup = selectionGroup;
            this.selectionBoxOverlay = selectionBoxOverlay;
            this.dragThresholdPixels = dragThresholdPixels;
            this.unitLayer = unitLayer;
            this.maxRaycastDistance = maxRaycastDistance;
        }

        public void OnLeftMouseDown(Vector2 mousePosition) {
            isLeftPointerDown = true;
            isBoxSelecting = false;
            boxStartScreenPosition = mousePosition;
        }

        public void OnLeftMouseHeld(Vector2 mousePosition) {
            if (!isLeftPointerDown) {
                return;
            }

            if (!isBoxSelecting) {
                if (Vector2.Distance(boxStartScreenPosition, mousePosition) < dragThresholdPixels) {
                    return;
                }

                isBoxSelecting = true;
            }
            
            selectionBoxOverlay?.Show(CalculateScreenRect(boxStartScreenPosition, mousePosition));
        }

        public void OnLeftMouseUp(Vector2 mousePosition, bool shiftHeld) {
            if (!isLeftPointerDown) {
                return;
            }

            if (isBoxSelecting) {
                Rect rect = CalculateScreenRect(boxStartScreenPosition, mousePosition);
                HandleBoxSelection(rect, shiftHeld);
            } else {
                HandleSelectionClick(mousePosition, shiftHeld);
            }
            
            selectionBoxOverlay?.Hide();

            isLeftPointerDown = false;
            isBoxSelecting = false;
        }

        public void ClearSelection() {
            selectionGroup?.Clear();
        }
        
        private Rect CalculateScreenRect(Vector2 start, Vector2 current) {
            float xMin = Mathf.Min(start.x, current.x);
            float xMax = Mathf.Max(start.x, current.x);
            float yMin = Mathf.Min(start.y, current.y);
            float yMax = Mathf.Max(start.y, current.y);
            return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
        }
        
        private void HandleBoxSelection(Rect rect, bool shiftHeld) {
            var playerUnitsInBox = new List<Unit>();
            var uncontrolledInBox = new List<UnitScreenPosition>();
            
            foreach (var unit in UnitRegistry.Units) {
                if (unit == null || unit.IsDead) continue;
                
                Vector3 screenPoint = MainCamera.WorldToScreenPoint(unit.transform.position);
                if (screenPoint.z < 0f) continue;
                
                Vector2 screenPosition = new Vector2(screenPoint.x, screenPoint.y);
                if (!rect.Contains(screenPosition)) continue;
                
                if (unit.IsPlayerControlled) {
                    playerUnitsInBox.Add(unit);
                } else {
                    uncontrolledInBox.Add(new UnitScreenPosition(unit, screenPosition));
                }
            }

            if (playerUnitsInBox.Count > 0) {
                if (shiftHeld && selectionGroup.HasPlayerControlledUnits()) {
                    selectionGroup.AddRange(playerUnitsInBox);
                } else {
                    selectionGroup.SelectExclusiveRange(playerUnitsInBox);
                }
                return;
            }

            if (shiftHeld) return;
            
            if (uncontrolledInBox.Count == 0) {
                selectionGroup.Clear();
                return;
            }

            Unit closest = FindClosestToBoxStart(uncontrolledInBox, boxStartScreenPosition);
            if (closest != null) {
                selectionGroup.SelectExclusive(closest);
            }
        }

        private Unit FindClosestToBoxStart(List<UnitScreenPosition> candidates, Vector2 boxStart) {
            Unit closest = null;
            float closestDistanceSqr = float.MaxValue;
            
            foreach (var candidate in candidates) {
                float distanceSqr = (candidate.ScreenPosition - boxStart).sqrMagnitude;
                if (distanceSqr < closestDistanceSqr) {
                    closestDistanceSqr = distanceSqr;
                    closest = candidate.Unit;
                }
            }
            
            return closest;
        }

        private void HandleSelectionClick(Vector2 mousePosition, bool shiftHeld) {
            if (!MainCamera) return;
            
            Ray ray = MainCamera.ScreenPointToRay(mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, maxRaycastDistance, unitLayer, QueryTriggerInteraction.Ignore)) {
                return;
            }

            Unit unit = hit.collider.GetComponentInParent<Unit>();
            if (!unit || unit.IsDead) {
                return;
            }

            if (shiftHeld) {
                if (unit.IsPlayerControlled && selectionGroup.HasPlayerControlledUnits()) {
                    selectionGroup.Toggle(unit);
                }
                return;
            }

            selectionGroup.SelectExclusive(unit);
        }

        private readonly struct UnitScreenPosition {
            public UnitScreenPosition(Unit unit, Vector2 screenPosition) {
                Unit = unit;
                ScreenPosition = screenPosition;
            }
            public Unit Unit { get; }
            public Vector2 ScreenPosition { get; }
        }
    }
}