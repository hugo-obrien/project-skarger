using _Project.Scripts.Controllers.ContextActions;
using _Project.Scripts.UI;
using _Project.Scripts.UI.Console;
using _Project.Scripts.Units;
using _Project.Scripts.Utils;
using UnityEngine;

namespace _Project.Scripts.Controllers {
    /// <summary>
    /// Detects a right click, resolves what was clicked into a context and delegates the
    /// behavior to the first <see cref="IContextAction"/> strategy able to handle it.
    /// Adding new right-click behavior does not require touching this class.
    /// </summary>
    [RequireComponent(typeof(SelectionGroup))]
    public class RightClickHandler : MonoBehaviour {
        [Header("Camera")]
        [SerializeField] private Camera camera;
        [SerializeField] private InteractiveLayerEntry[] interactiveLayers;
        [SerializeField] [Min(1f)] private float maxRaycastDistance = 1000f;

        [Header("Markers")]
        [SerializeField] private MoveMarkerVisual moveMarkerPrefab;

        [Header("Context actions")]
        [SerializeField] private ContextAction[] contextActions;

        private SelectionGroup _selectionGroup;
        private GameControls _controls;
        private DebugConsole _console;
        private LayerMask _combinedClickedMask;
        private LayerMask _unitLayer;
        private MoveMarkerPresenter _moveMarkerPresenter;
        private RightClickContextBuilder _contextBuilder;
        private ContextActionResolver _actionResolver;

        private void Awake() {
            if (!TryGetComponent(out _selectionGroup)) {
                LogUtil.Error(nameof(RightClickHandler), nameof(Awake), "Selection group not found");
            }

            CacheInteractiveLayers();

            _moveMarkerPresenter = new MoveMarkerPresenter(moveMarkerPrefab);
            _contextBuilder = new RightClickContextBuilder(interactiveLayers);
            _actionResolver = new ContextActionResolver(contextActions);
        }

        private void OnEnable() {
            _controls ??= new GameControls();
            _controls.Enable();
            _console ??= FindAnyObjectByType<DebugConsole>();
        }

        private void OnDisable() {
            _controls?.Disable();
        }

        private void OnDestroy() {
            _controls?.Dispose();
            _moveMarkerPresenter?.ClearAll();
        }

        private void Update() {
            if (camera == null) {
                camera = Camera.main;
                if (camera == null) {
                    LogUtil.Error(nameof(RightClickHandler), nameof(Update), "Camera not found");
                    return;
                }
            }

            _moveMarkerPresenter.Tick();

            if (_console != null && _console.IsOpen) return;

            if (_controls.GameControl.Command.WasPressedThisFrame() && !UiUtils.IsPointerOverUI() &&
                _selectionGroup.HasPlayerControlledUnits()) {
                HandleRightClick();
            }
        }

        private void HandleRightClick() {
            Ray ray = camera.ScreenPointToRay(_controls.GameControl.PointerPosition.ReadValue<Vector2>());

            RaycastHit[] hits = Physics.RaycastAll(ray, maxRaycastDistance, _combinedClickedMask,
                QueryTriggerInteraction.Ignore);

            if (hits.Length == 0) return;

            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            RightClickContext context = _contextBuilder.TryBuild(hits);
            if (context == null) return;

            var services = new RightClickContextServices(_selectionGroup, _unitLayer, _moveMarkerPresenter,
                _controls.GameControl.Modifier.IsPressed());

            _actionResolver.ResolveAndExecute(context, services);
        }

        private void CacheInteractiveLayers() {
            foreach (var entry in interactiveLayers) {
                _combinedClickedMask |= entry.layerMask;
                if (entry.type == InteractiveLayerType.Unit) {
                    _unitLayer = entry.layerMask;
                }
            }
        }
    }
}