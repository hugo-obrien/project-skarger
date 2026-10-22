using System;
using UnityEngine;

namespace _Project.Scripts.Controllers {
    public enum InteractiveLayerType {
        Ground,
        Unit
    }

    [Serializable]
    public sealed class InteractiveLayerEntry {
        public InteractiveLayerType type;
        public LayerMask layerMask;
    }
}
