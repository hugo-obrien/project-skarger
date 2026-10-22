using _Project.Scripts.Units;
using _Project.Scripts.Utils;
using UnityEngine;

namespace _Project.Scripts.Controllers.ContextActions {
    /// <summary>
    /// Turns a raycast result into a <see cref="RightClickContext"/> by resolving which
    /// interactive layer was hit and whether the hit object carries a live unit.
    /// </summary>
    public sealed class RightClickContextBuilder {
        private readonly InteractiveLayerEntry[] interactiveLayers;

        public RightClickContextBuilder(InteractiveLayerEntry[] interactiveLayers) {
            this.interactiveLayers = interactiveLayers;
        }

        public RightClickContext TryBuild(RaycastHit[] hits) {
            for (int i = 0; i < hits.Length; i++) {
                Collider collider = hits[i].collider;
                if (collider == null) {
                    LogUtil.Warn(nameof(RightClickContextBuilder), nameof(TryBuild), "Hit collider is null");
                    continue;
                }

                InteractiveLayerType? layerType = GetLayerType(collider.gameObject.layer);
                if (layerType == null) continue;

                return layerType.Value switch {
                    InteractiveLayerType.Unit => BuildUnitContext(hits[i], collider),
                    _ => new RightClickContext(hits[i], null)
                };
            }

            return null;
        }

        private InteractiveLayerType? GetLayerType(int layer) {
            int layerBit = 1 << layer;

            foreach (var entry in interactiveLayers) {
                if ((entry.layerMask.value & layerBit) != 0) {
                    return entry.type;
                }
            }

            return null;
        }

        private RightClickContext BuildUnitContext(RaycastHit hit, Collider collider) {
            Unit unit = collider.GetComponentInParent<Unit>();
            if (unit == null) {
                LogUtil.Warn(nameof(RightClickContextBuilder), nameof(BuildUnitContext),
                    "Hit collider is on unit layer but has no Unit component");
                return new RightClickContext(hit, null);
            }

            return unit.IsDead ? null : new RightClickContext(hit, unit);
        }
    }
}
