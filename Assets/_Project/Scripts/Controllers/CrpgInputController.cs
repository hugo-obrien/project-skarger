using _Project.Scripts.UI;
using _Project.Scripts.UI.Console;
using _Project.Scripts.Units;
using _Project.Scripts.Utils;
using UnityEngine;

namespace _Project.Scripts.Controllers
{
    [RequireComponent(typeof(SelectionGroup))]
    [RequireComponent(typeof(SelectionBoxOverlay))]
    [RequireComponent(typeof(RightClickHandler))]
    public class CrpgInputController : MonoBehaviour
    {
        [Header("Camera")] [SerializeField] private Camera mainCamera;

        [Header("Layers")] [SerializeField] private LayerMask unitLayer = 1;
        
        [Header("Selection box")] [SerializeField]
        private SelectionBoxOverlay selectionBoxOverlay;

        [SerializeField] [Min(1f)] private float dragThresholdPixels;

        [SerializeField] [Min(1f)] private float maxRaycastDistance = 1000f;

        private SelectionGroup selectionGroup;
        private SelectionHandler selectionHandler;
        private RightClickHandler rightClickHandler;
        private GameControls controls;
        private DebugConsole console;
        
        private void Awake()
        {
            if (!TryGetComponent(out selectionGroup))
            {
                LogUtil.Error("CrpgInputController", "Awake", "Requires SelectionGroup");
            }

            if (!TryGetComponent(out selectionBoxOverlay))
            {
                LogUtil.Error("CrpgInputController", "Awake", "Requires SelectionBoxOverlay");
            }

            selectionHandler = new SelectionHandler(selectionGroup, selectionBoxOverlay, dragThresholdPixels,
                unitLayer, maxRaycastDistance)
            {
                MainCamera = mainCamera
            };
        }

        private void OnEnable()
        {
            controls ??= new GameControls();
            controls.Enable();
            console ??= FindAnyObjectByType<DebugConsole>();
        }

        private void OnDisable()
        {
            controls?.Disable();
        }

        private void OnDestroy()
        {
            controls?.Dispose();
        }

        private void Reset()
        {
            mainCamera = Camera.main;
        }

        private void Update()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null) return;
                selectionHandler.MainCamera = mainCamera;
            }

            if (console != null && console.IsOpen)
            {
                return;
            }

            if (controls.GameControl.CancelCommand.WasPressedThisFrame())
            {
                selectionHandler.ClearSelection();
                return;
            }

            if (controls.GameControl.Select.WasPressedThisFrame() && !UiUtils.IsPointerOverUI())
            {
                selectionHandler.OnLeftMouseDown(controls.GameControl.PointerPosition.ReadValue<Vector2>());
            }

            if (controls.GameControl.Select.IsPressed())
            {
                selectionHandler.OnLeftMouseHeld(controls.GameControl.PointerPosition.ReadValue<Vector2>());
            }

            if (controls.GameControl.Select.WasReleasedThisFrame())
            {
                selectionHandler.OnLeftMouseUp(controls.GameControl.PointerPosition.ReadValue<Vector2>(), controls.GameControl.Modifier.IsPressed());
            }
        }
    }
}