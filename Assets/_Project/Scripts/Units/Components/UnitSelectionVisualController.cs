using System;
using _Project.Scripts.UI;
using UnityEngine;

namespace _Project.Scripts.Units.Components
{
    /// <summary>
    /// Responsibility: visualization of the selected unit (marker at its feet) (SRP). 
    /// Subscribes to Unit events itself — the Unit knows nothing about markers or faction colors (ISP/DIP). 
    /// </summary>
    [DisallowMultipleComponent]
    public class UnitSelectionVisualController : MonoBehaviour
    {
        private const float SelectedColorBrighten = 0.5f;

        [SerializeField] private UnitSelectionVisual selectionVisual;

        private Unit unit;

        private void Awake()
        {
            if (!TryGetComponent(out unit))
            {
                Debug.LogError($"{nameof(UnitSelectionVisualController)} requires {nameof(Unit)} on the GameObject", this);
            }
        }

        private void OnEnable()
        {
            if (unit != null)
            {
                unit.FactionChanged += UpdateVisual;
                unit.CombatModeChanged += UpdateVisual;
                unit.SelectionChanged += UpdateVisual;
                unit.Died += HandleDeath;
            }
        }
        
        private void OnDisable()
        {
            if (unit != null)
            {
                unit.FactionChanged -= UpdateVisual;
                unit.CombatModeChanged -= UpdateVisual;
                unit.SelectionChanged -= UpdateVisual;
                unit.Died -= HandleDeath;
            }
        }

        private void Start()
        {
            UpdateVisual();
        }

        public void Refresh()
        {
            UpdateVisual();
        }

        private void HandleDeath()
        {
            if (selectionVisual != null)
            {
                selectionVisual.Hide();
            }
        }

        private void UpdateVisual()
        {
            if (!selectionVisual || unit == null) return;

            if (unit.IsDead)
            {
                selectionVisual.Hide();
                return;
            }

            if (unit.IsSelected)
            {
                var baseColor = Factions.UnitFactionColors.GetSelectionColor(unit.Faction);
                var brightColor = Color.Lerp(baseColor, Color.white, SelectedColorBrighten);
                selectionVisual.Show(transform, brightColor);
            } else if (unit.IsPlayerControlled || unit.IsInCombat)
            {
                var defaultColor = Factions.UnitFactionColors.GetSelectionColor(unit.Faction);
                selectionVisual.Show(transform, defaultColor);
            }
            else
            {
                selectionVisual.Hide();
            }
        }
    }
}