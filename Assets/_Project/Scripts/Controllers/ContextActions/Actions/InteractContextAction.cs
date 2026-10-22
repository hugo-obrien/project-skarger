using System.Collections.Generic;
using _Project.Scripts.Units;
using _Project.Scripts.Units.Commands;
using _Project.Scripts.Units.Factions;
using UnityEngine;

namespace _Project.Scripts.Controllers.ContextActions.Actions {
    /// <summary>
    /// Right-click on an allied or neutral unit: approach it and play a greeting.
    /// </summary>
    [CreateAssetMenu(menuName = "Context Actions/Interact")]
    public sealed class InteractContextAction : ContextAction {
        [SerializeField] [TextArea] private string greeting = "Hello, traveller!";

        public override bool CanHandle(RightClickContext context) {
            Unit target = context.TargetUnit;
            if (target == null || target.IsDead) return false;

            return target.Faction == UnitFaction.Allied || target.Faction == UnitFaction.Neutral;
        }

        public override void Execute(RightClickContext context, RightClickContextServices services) {
            Unit target = context.TargetUnit;

            IReadOnlyList<Unit> units = GetSelectedUnits(services);
            if (units.Count == 0) return;

            IReadOnlyList<Vector3> destinations = BuildFormation(services, units, CalculateApproachPoint(units, target));
            bool appendRequested = services.AppendRequested;

            for (int i = 0; i < units.Count; i++) {
                Unit unit = units[i];
                if (unit == null) continue;
                
                unit.Commands.Issue(new MoveCommand(unit, unit.Combat, destinations[i]), CommandSource.Player, appendRequested);
                if (i == 0) {
                    unit.Commands.Issue(new SpeechBubbleCommand(target, greeting), CommandSource.Player, true);
                }
            }
        }

        private static Vector3 CalculateApproachPoint(IReadOnlyList<Unit> units, Unit target) {
            Vector3 targetPosition = target.transform.position;

            Vector3 groupCenter = Vector3.zero;
            foreach (var unit in units) {
                groupCenter += unit.transform.position;
            }
            groupCenter /= units.Count;

            Vector3 offset = groupCenter - targetPosition;
            offset.y = 0f;
            Vector3 direction = offset.sqrMagnitude > 0.001f ? offset.normalized : -target.transform.forward;

            return targetPosition + direction * target.Stats.interactionDistance;
        }
    }
}
