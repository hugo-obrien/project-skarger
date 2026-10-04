using _Project.Scripts.UI;
using UnityEngine;

namespace _Project.Scripts.Units {

    /// <summary>
    /// Ответственность: визуализация выбранного юнита (маркер под ногами) (SRP).
    /// Подписывается на события Unit сам — Unit не знает ни о маркерах, ни о цветах фракций (ISP/DIP).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UnitSelectionVisualController : MonoBehaviour {

        private const float SelectedColorBrighten = 0.5f;

        [SerializeField] private UnitSelectionVisual selectionVisual;

        private Unit unit;

        /// <summary>Принудительный пересчёт маркера (используется, когда событие уже было отправлено до подписки).</summary>
        public void Refresh() {
            UpdateVisual();
        }

        private void Awake() {
            if (!TryGetComponent(out unit)) {
                Debug.LogError($"{nameof(UnitSelectionVisualController)} requires {nameof(Unit)} on the same GameObject", this);
            }
        }

        private void OnEnable() {
            if (unit != null) {
                unit.FactionChanged += UpdateVisual;
                unit.CombatModeChanged += UpdateVisual;
                unit.SelectionChanged += UpdateVisual;
                unit.Died += HandleDeath;
            }
        }

        private void OnDisable() {
            if (unit != null) {
                unit.FactionChanged -= UpdateVisual;
                unit.CombatModeChanged -= UpdateVisual;
                unit.SelectionChanged -= UpdateVisual;
                unit.Died -= HandleDeath;
            }
        }

        private void Start() {
            UpdateVisual();
        }

        private void HandleDeath() {
            if (selectionVisual != null) {
                selectionVisual.Hide();
            }
        }

        private void UpdateVisual() {
            if (!selectionVisual || unit == null) return;

            if (unit.IsDead) {
                selectionVisual.Hide();
                return;
            }

            if (unit.IsSelected) {
                Color baseColor = Factions.UnitFactionColors.GetSelectionColor(unit.Faction);
                Color brightColor = Color.Lerp(baseColor, Color.white, SelectedColorBrighten);
                selectionVisual.Show(transform, brightColor);
            } else if (unit.IsPlayerControlled || unit.IsInCombat) {
                Color defaultColor = Factions.UnitFactionColors.GetSelectionColor(unit.Faction);
                selectionVisual.Show(transform, defaultColor);
            } else {
                selectionVisual.Hide();
            }
        }
    }
}
