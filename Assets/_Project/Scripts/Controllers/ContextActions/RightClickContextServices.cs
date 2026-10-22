using _Project.Scripts.UI;
using _Project.Scripts.Units;
using UnityEngine;

namespace _Project.Scripts.Controllers.ContextActions {
    /// <summary>
    /// Shared infrastructure available to every context action: the current selection,
    /// interactive layers and the marker presentation. Rebuilt on each right click so
    /// actions always observe up-to-date input state.
    /// </summary>
    public sealed class RightClickContextServices {
        public RightClickContextServices(SelectionGroup selectionGroup, LayerMask unitLayer,
            MoveMarkerPresenter moveMarkers, bool appendRequested) {
            SelectionGroup = selectionGroup;
            UnitLayer = unitLayer;
            MoveMarkers = moveMarkers;
            AppendRequested = appendRequested;
        }

        public SelectionGroup SelectionGroup { get; }
        public LayerMask UnitLayer { get; }
        public MoveMarkerPresenter MoveMarkers { get; }

        /// <summary>
        /// Modifier held during the click: commands are appended to the queue instead of replacing it.
        /// </summary>
        public bool AppendRequested { get; }
    }
}
