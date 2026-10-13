using UnityEngine;
using _Project.Scripts.UI;
using _Project.Scripts.Units;
using _Project.Scripts.Utils;
using UnityEngine.InputSystem;

namespace _Project.Scripts.Controllers
{
    [RequireComponent(typeof(SelectionGroup))]
    [RequireComponent(typeof(SelectionBoxOverlay))]
    [RequireComponent(typeof(RightClickHandler))]
    public class CrpgInputController : MonoBehaviour
    {
        [Header("Camera")] [SerializeField] private UnityEngine.Camera mainCamera;

        [Header("Layers")] [SerializeField] private LayerMask unitLayer = 1;

        [Header("Selection box")] [SerializeField]
        private SelectionBoxOverlay selectionBoxOverlay;

        [SerializeField] [Min(1f)] private float dragThresholdPixels;

        [SerializeField] [Min(1f)] private float maxRaycastDistance = 1000f;

        private SelectionGroup selectionGroup;
        private SelectionHandler selectionHandler;
        private RightClickHandler rightClickHandler;

        private GameplayControls controls;

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

            InitializeInput();
        }

        private void Reset()
        {
            mainCamera = UnityEngine.Camera.main;
        }

        private void OnEnable()
        {
            controls?.Gameplay.Enable();
        }

        private void OnDisable()
        {
            controls?.Gameplay.Disable();
        }

        private void OnDestroy()
        {
            controls?.Dispose();
        }

        private void Update()
        {
            if (mainCamera == null)
            {
                mainCamera = UnityEngine.Camera.main;
                if (mainCamera == null) return;
                selectionHandler.MainCamera = mainCamera;
            }

            if (controls.Gameplay.Cancel.WasPressedThisFrame())
            {
                selectionHandler.ClearSelection();
                return;
            }

            var pointerPosition = controls.Gameplay.Pointer.ReadValue();

            if (controls.Gameplay.LeftClick.WasPressedThisFrame() && !UiUtils.IsPointerOverUI())
            {
                selectionHandler.OnLeftMouseDown(pointerPosition);
            }

            if (controls.Gameplay.LeftClick.isPressed)
            {
                selectionHandler.OnLeftMouseHeld(pointerPosition);
            }

            if (controls.Gameplay.LeftClick.WasReleasedThisFrame())
            {
                selectionHandler.OnLeftMouseUp(pointerPosition, UiUtils.IsShiftPressed());
            }
        }

        private void InitializeInput()
        {
            if (controls != null)
            {
                return;
            }

            controls = new GameplayControls();
        }
    }
}
