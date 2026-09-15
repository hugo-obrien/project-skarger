using System;
using _Project.Scripts.Combat;
using _Project.Scripts.UI;
using _Project.Scripts.Units.Factions;
using UnityEngine;

namespace _Project.Scripts.Units.AI
{
    [RequireComponent(typeof(Unit))]
    [RequireComponent(typeof(UnitCombat))]
    public class UnitAI : MonoBehaviour
    {
        private Unit unit;
        private UnitCombat combat;
        private float nextSearchTime;

        private void Awake()
        {
            unit = GetComponent<Unit>();
            combat = GetComponent<UnitCombat>();
        }

        private void Update()
        {
            if (!unit.Stats.combat.aiControlled) return;
            if (unit.IsDead) return;
            if (combat.IsActive) return;

            if (Time.time < nextSearchTime) return;
            nextSearchTime = Time.time + unit.Stats.combat.targetSearchInterval;

            TryFindTarget();
        }

        private void TryFindTarget()
        {
            var allUnits = UnitRegistry.Units;
            if (allUnits == null || allUnits.Count == 0) return;

            Vector3 myPos = unit.transform.position;
            float perceptionRadius = unit.Stats.combat.perceptionRadius;
            float perceptionSqr = perceptionRadius * perceptionRadius;

            Unit bestTarget = null;
            float bestDistanceSqr = float.MaxValue;

            foreach (var candidate in allUnits)
            {
                if (candidate == null || candidate.IsDead || candidate == unit) continue;

                if (!FactionRelations.AreHostile(unit.Faction, candidate.Faction)) continue;

                float distanceSqr = (candidate.transform.position - myPos).sqrMagnitude;
                if (distanceSqr > perceptionSqr) continue;
                
                if (distanceSqr < bestDistanceSqr)
                {
                    bestDistanceSqr = distanceSqr;
                    bestTarget = candidate;
                }
            }

            if (bestTarget != null)
            {
                combat.Attack(bestTarget, force: false);
                
                unit.UpdateSelectionVisual();
            }
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!TryGetComponent<Unit>(out var u) || !u.Stats.combat.aiControlled) return;

            Gizmos.color = new Color(0.11f, 0.11f, 0.7f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, u.Stats.combat.perceptionRadius);
        }
#endif
    }
}