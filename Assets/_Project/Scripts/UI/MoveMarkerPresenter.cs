using System.Collections.Generic;
using _Project.Scripts.Units;
using _Project.Scripts.Utils;
using UnityEngine;

namespace _Project.Scripts.UI {
    /// <summary>
    /// Owns the lifetime of move markers: spawning them at formation destinations and
    /// fading them out once every unit that was ordered to move has finished its queue.
    /// </summary>
    public sealed class MoveMarkerPresenter {
        private readonly MoveMarkerVisual markerPrefab;
        private readonly List<MoveMarkerVisual> activeMarkers = new List<MoveMarkerVisual>();

        private IReadOnlyList<Unit> movingUnits;
        private float fadeDuration;

        public MoveMarkerPresenter(MoveMarkerVisual markerPrefab) {
            this.markerPrefab = markerPrefab;
        }

        public bool HasActiveMarkers => activeMarkers.Count > 0;

        public void Show(IReadOnlyList<Vector3> points, Vector3 normal, Color color, float offset,
            float autoFadeDuration, IReadOnlyList<Unit> movingUnits) {
            ClearAll();

            if (markerPrefab == null) {
                LogUtil.Warn(nameof(MoveMarkerPresenter), nameof(Show), "Move marker prefab is not assigned");
                return;
            }

            this.movingUnits = movingUnits;
            fadeDuration = autoFadeDuration;

            Vector3 normalizedNormal = normal.normalized;
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normalizedNormal);

            foreach (Vector3 point in points) {
                Vector3 position = point + normalizedNormal * offset;
                MoveMarkerVisual marker = Object.Instantiate(markerPrefab, position, rotation);
                marker.Setup(position, rotation, color);
                activeMarkers.Add(marker);
            }
        }

        public void Tick() {
            if (!HasActiveMarkers || movingUnits == null) return;

            foreach (Unit unit in movingUnits) {
                if (unit == null) continue;
                if (!unit.Commands.IsIdle) return;
            }

            FadeOutAll();
        }

        public void ClearAll() {
            DestroyMarkers();
            movingUnits = null;
        }

        private void FadeOutAll() {
            foreach (var marker in activeMarkers) {
                if (marker != null) {
                    marker.FadeOutAndDestroy(fadeDuration);
                }
            }

            activeMarkers.Clear();
            movingUnits = null;
        }

        private void DestroyMarkers() {
            foreach (var marker in activeMarkers) {
                if (marker != null) {
                    Object.Destroy(marker.gameObject);
                }
            }

            activeMarkers.Clear();
        }
    }
}
