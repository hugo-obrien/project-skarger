using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Utils;
using UnityEngine;

namespace _Project.Scripts.Units {
    public class SelectionGroup : MonoBehaviour {
        private readonly HashSet<Unit> selectedUnits = new();

        public event Action SelectionChanged;

        public IReadOnlyCollection<Unit> SelectedUnits => selectedUnits;

        public bool IsSelected(Unit unit) {
            return unit != null && selectedUnits.Contains(unit);
        }

        public bool AnySelected() {
            return selectedUnits.Count > 0;
        }

        public bool HasPlayerControlledUnits() {
            return selectedUnits.Any(unit => unit != null && unit.IsPlayerControlled);
        }
        
        public IReadOnlyList<Unit> GetPlayerControlledUnits() {
            return selectedUnits
                .Where(unit => unit != null && unit.IsPlayerControlled)
                .ToList();
        }
        

        public void SelectExclusive(Unit unit) {
            if (unit == null) {
                LogUtil.Warn("SelectionGroup", "SelectExclusive", "Unit is null");
                return;
            }

            if (selectedUnits.Count == 1 && selectedUnits.Contains(unit)) {
                return;
            }

            ClearInternal();
            AddInternal(unit);
            RaiseSelectionChanged();
        }

        public void Toggle(Unit unit) {
            if (unit == null) {
                LogUtil.Warn("SelectionGroup", "Toggle", "Unit is null");
                return;
            }
            
            if (selectedUnits.Contains(unit)) {
                RemoveInternal(unit, updateVisual: true);
            } else {
                AddInternal(unit);
            }

            RaiseSelectionChanged();
        }
        
        public void Add(Unit unit) {
            if (unit == null) {
                LogUtil.Warn("SelectionGroup", "Add", "Unit is null");
                return;
            }

            if (AddInternal(unit)) {
                RaiseSelectionChanged();
            }
        }

        public void Remove(Unit unit) {
            if (unit  == null) {
                return;
            }

            if (RemoveInternal(unit, updateVisual: true)) {
                RaiseSelectionChanged();
            }
        }

        public void Clear() {
            if (selectedUnits.Count == 0) {
                return;
            }

            ClearInternal();
            RaiseSelectionChanged();
        }

        public bool SelectedUserController() {
            return selectedUnits.First().Faction == UnitFaction.User;
        }
        
        private bool AddInternal(Unit unit) {
            if (unit == null) {
                return false;
            }

            if (!selectedUnits.Add(unit)) {
                return false;
            }
            
            unit.SetSelected(true);
            unit.Destroyed += OnUnitDestroyed;

            return true;
        }
        
        private void OnUnitDestroyed(Unit unit) {
            if (unit == null) {
                return;
            }

            RemoveInternal(unit, updateVisual: false);
            RaiseSelectionChanged();
        }
        
        private bool RemoveInternal(Unit unit, bool updateVisual) {
            if (!selectedUnits.Remove(unit)) {
                return false;
            }

            if (unit != null) {
                if (updateVisual) {
                    unit.SetSelected(false);
                }

                unit.Destroyed -= OnUnitDestroyed;
            }

            return true;
        }

        private void ClearInternal() {
            foreach (var unit in selectedUnits.ToList()) {
                if (unit == null) {
                    continue;
                }
                
                unit.SetSelected(false);
                unit.Destroyed -= OnUnitDestroyed;
            }
            
            selectedUnits.Clear();
        }

        private void RaiseSelectionChanged() {
            SelectionChanged?.Invoke();
        }
    }
}
