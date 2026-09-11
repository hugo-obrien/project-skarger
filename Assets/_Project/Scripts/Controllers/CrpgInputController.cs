using _Project.Scripts.UI;
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

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                selectionHandler.ClearSelection();
                return;
            }

            if (Input.GetMouseButtonDown(0) && !UiUtils.IsPointerOverUI())
            {
                selectionHandler.OnLeftMouseDown(Input.mousePosition);
            }

            if (Input.GetMouseButton(0))
            {
                selectionHandler.OnLeftMouseHeld(Input.mousePosition);
            }

            if (Input.GetMouseButtonUp(0))
            {
                selectionHandler.OnLeftMouseUp(Input.mousePosition, UiUtils.IsShiftPressed());
            }
        }
    }
}