using System.Collections.Generic;
using _Project.Scripts.Units;
using _Project.Scripts.Utils;
using UnityEngine;

namespace _Project.Scripts.Controllers.ContextActions {
    /// <summary>
    /// Base class for context actions: keeps shared serialized group-movement settings
    /// and exposes helpers reused by concrete strategies.
    /// </summary>
    public abstract class ContextAction : ScriptableObject, IContextAction {
        [Header("Group movement")]
        [SerializeField] [Min(0.1f)] [Tooltip("Distance between units in group")]
        private float unitSpacing = 1.25f;
        [SerializeField] [Range(0.1f, 0.9f)] [Tooltip("Occupance check radius factor of unit spacing")]
        private float occupancyRadiusFactor = 0.45f;

        public virtual int Priority => (int)ContextActionPriority.Normal;

        public abstract bool CanHandle(RightClickContext context);

        public abstract void Execute(RightClickContext context, RightClickContextServices services);

        protected static IReadOnlyList<Unit> GetSelectedUnits(RightClickContextServices services) {
            return services.SelectionGroup.GetPlayerControlledUnits();
        }

        protected IReadOnlyList<Vector3> BuildFormation(RightClickContextServices services,
            IReadOnlyList<Unit> units, Vector3 target) {
            float occupancyRadius = unitSpacing * occupancyRadiusFactor;
            return FormationResolver.BuildDestinations(units, target, unitSpacing, services.UnitLayer, occupancyRadius);
        }
    }
}
