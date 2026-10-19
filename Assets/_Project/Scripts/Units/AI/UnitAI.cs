using _Project.Scripts.Combat;
using _Project.Scripts.UI;
using _Project.Scripts.Units.Commands;
using _Project.Scripts.Units.Components;
using _Project.Scripts.Units.Factions;
using UnityEngine;

namespace _Project.Scripts.Units.AI {
    [RequireComponent(typeof(Unit))]
    [RequireComponent(typeof(UnitCombat))]
    public class UnitAI : MonoBehaviour {
        private Unit _unit;
        private UnitCombat _combat;
        private UnitSelectionVisualController _selectionVisualController;
        private float _nextSearchTime;

        private void OnEnable() {
            if (_unit != null) {
                _unit.ReceivedDamage += OnReceiveDamage;
            }
        }

        private void OnDisable() {
            if (_unit != null) {
                _unit.ReceivedDamage -= OnReceiveDamage;
            }
        }

        private void Awake() {
            _unit = GetComponent<Unit>();
            _combat = GetComponent<UnitCombat>();
            _selectionVisualController = GetComponent<UnitSelectionVisualController>();
        }

        private void Update() {
            if (!_unit.Stats.combat.aiControlled) return;
            if (_unit.IsDead) return;
            if (!_unit.Commands.IsIdle) return;

            if (Time.time < _nextSearchTime) return;
            _nextSearchTime = Time.time + _unit.Stats.combat.targetSearchInterval;

            TryFindTarget();
        }

        private void OnReceiveDamage(IDamageDealer causer) {
            if (causer is Unit damageDealer) {
                _unit.Commands.Issue(new AttackCommand(_combat, damageDealer), CommandSource.AI,
                    appendRequested: false);
            }
        }

        private void TryFindTarget() {
            var allUnits = UnitRegistry.Units;
            if (allUnits == null || allUnits.Count == 0) return;

            Vector3 myPos = _unit.transform.position;
            float perceptionRadius = _unit.Stats.combat.perceptionRadius;
            float perceptionSqr = perceptionRadius * perceptionRadius;

            Unit bestTarget = null;
            float bestDistanceSqr = float.MaxValue;

            foreach (var candidate in allUnits) {
                if (candidate == null || candidate.IsDead || candidate == _unit) continue;

                if (!FactionRelations.AreHostile(_unit.Faction, candidate.Faction)) continue;

                float distanceSqr = (candidate.transform.position - myPos).sqrMagnitude;
                if (distanceSqr > perceptionSqr) continue;

                if (distanceSqr < bestDistanceSqr) {
                    bestDistanceSqr = distanceSqr;
                    bestTarget = candidate;
                }
            }

            if (bestTarget != null) {
                _unit.Commands.Issue(new AttackCommand(_combat, bestTarget), CommandSource.AI, appendRequested: false);

                _selectionVisualController?.Refresh();
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos() {
            if (!TryGetComponent<Unit>(out var u) || !u.Stats.combat.aiControlled) return;

            Gizmos.color = new Color(0.11f, 0.11f, 0.7f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, u.Stats.combat.perceptionRadius);
        }
#endif
    }
}