using System.Collections.Generic;
using _Project.Scripts.Units;
using _Project.Scripts.Units.Commands;
using UnityEngine;

namespace _Project.Scripts.Controllers.ContextActions {
    /// <summary>
    /// Default right-click behavior: move the selected group to the clicked point.
    /// Handles ground clicks and clicks on player-controlled units.
    /// </summary>
    [CreateAssetMenu(menuName = "Context Actions/Move")]
    public sealed class MoveContextAction : ContextAction {
        [SerializeField] private Color markerColor = Color.darkGreen;
        [SerializeField] [Min(0f)] private float markerOffset = 0.03f;
        [SerializeField] [Min(0f)] private float markerFadeDuration = 0.25f;

        public override int Priority => (int)ContextActionPriority.Fallback;

        public override bool CanHandle(RightClickContext context) {
            return context.TargetUnit == null || context.TargetUnit.IsPlayerControlled;
        }

        public override void Execute(RightClickContext context, RightClickContextServices services) {
            Vector3 destination = context.TargetUnit != null ? context.TargetUnit.transform.position : context.Point;

            IReadOnlyList<Unit> units = GetSelectedUnits(services);
            if (units.Count == 0) return;

            IReadOnlyList<Vector3> destinations = BuildFormation(services, units, destination);
            bool appendRequested = services.AppendRequested;

            for (int i = 0; i < units.Count; i++) {
                Unit unit = units[i];
                if (unit == null) continue;

                unit.Commands.Issue(new MoveCommand(unit, unit.Combat, destinations[i]), CommandSource.Player,
                    appendRequested);
            }

            services.MoveMarkers.Show(destinations, context.Normal, markerColor, markerOffset, markerFadeDuration,
                units);
        }
    }
}
