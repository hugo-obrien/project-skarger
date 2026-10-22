using _Project.Scripts.Units;
using _Project.Scripts.Units.Commands;
using _Project.Scripts.Units.Factions;
using UnityEngine;

namespace _Project.Scripts.Controllers.ContextActions {
    /// <summary>
    /// Right-click on an enemy unit: order the selected group to attack it.
    /// </summary>
    [CreateAssetMenu(menuName = "Context Actions/Attack")]
    public sealed class AttackContextAction : ContextAction {
        public override int Priority => (int)ContextActionPriority.High;

        public override bool CanHandle(RightClickContext context) {
            Unit target = context.TargetUnit;
            return target != null && !target.IsDead && target.Faction == UnitFaction.Enemy;
        }

        public override void Execute(RightClickContext context, RightClickContextServices services) {
            Unit target = context.TargetUnit;

            foreach (Unit unit in GetSelectedUnits(services)) {
                if (unit == null) continue;

                unit.Commands.Issue(new AttackCommand(unit.Combat, target), CommandSource.Player,
                    services.AppendRequested);
            }
        }
    }
}
