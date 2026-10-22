using _Project.Scripts.Units;
using UnityEngine;

namespace _Project.Scripts.Controllers.ContextActions {
    /// <summary>
    /// Immutable snapshot of a right-click target: raw hit data plus the resolved
    /// unit (if any). Passed to every context action.
    /// </summary>
    public sealed class RightClickContext {
        public RightClickContext(RaycastHit hit, Unit targetUnit) {
            Hit = hit;
            TargetUnit = targetUnit;
        }

        public RaycastHit Hit { get; }
        public Unit TargetUnit { get; }
        public Vector3 Point => Hit.point;
        public Vector3 Normal => Hit.normal;
    }
}
