using _Project.Scripts.UI.Console;
using _Project.Scripts.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.Controllers
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        private const float DefaultEdgeThresholdPixels = 10f;
        private const float MinInputMagnitude = 0.001f;
        private const float MaxInputMagnitude = 1f;
        private const float MaxZoomInputPerFrame = 1f;

        [Header("Move")] [SerializeField] private float moveSpeed = 2f;

        [SerializeField] [Tooltip("Screen border threshold")]
        private float edgeThresholdPixels = DefaultEdgeThresholdPixels;

        [SerializeField] private bool disableEdgeScrollWhileMouseRotating = true;

        [Header("Rotate")] [SerializeField] private float keyboardRotationSpeed = 90f;

        [SerializeField] private float mouseRotationSensitivity = 0.3f;

        [Header("Zoom")] [SerializeField] private float zoomSpeed = 5f;
        [SerializeField] private float minZoomOffset = 3f;
        [SerializeField] private float maxZoomOffset = 13f;
        [SerializeField] private LayerMask groundLayer;

        private CameraControls controls;

        private float pendingZoomInput;
        private Vector2 pendingMouseDelta;

        private DebugConsole console;

        private void Awake()
        {
            InitializeInput();

            console = FindAnyObjectByType<DebugConsole>();
        }

        private void OnEnable()
        {
            InitializeInput();
            controls.Enable();
        }

        private void OnDisable()
        {
            controls?.Disable();
        }

        private void OnDestroy()
        {
            controls?.Dispose();
        }

        private void Update()
        {
            if (console != null && console.IsOpen)
            {
                return;
            }
            
            if (controls == null)
            {
                LogUtil.Warn("CameraController", "Update", "Controls is null");
            }

            float deltaTime = Time.deltaTime;

            ApplyRotation(deltaTime);
            ApplyZoom();
            ApplyMovement(deltaTime);

            ClearFrameInput();
        }

        private void ClearFrameInput()
        {
            pendingZoomInput = 0f;
            pendingMouseDelta = Vector2.zero;
        }

        private void ApplyZoom()
        {
            float zoomInput = Mathf.Clamp(pendingZoomInput, -MaxZoomInputPerFrame, MaxZoomInputPerFrame);

            if (Mathf.Abs(zoomInput) < MinInputMagnitude)
            {
                return;
            }

            Vector3 delta = transform.forward * (zoomInput * zoomSpeed);
            Vector3 currentPos = transform.position;
            Vector3 nextPos = currentPos + delta;

            float currentGroundY = FoundGroundProjection(currentPos).y;
            float nextGroundY = FoundGroundProjection(nextPos).y;

            float currentHeight = currentPos.y - currentGroundY;
            float nextHeight = nextPos.y - nextGroundY;

            if (currentHeight <= minZoomOffset && zoomInput > 0) return;
            if (currentHeight >= maxZoomOffset && zoomInput < 0) return;

            float t = 1f;
            float targetHeight = -1f;

            if (zoomInput > 0 && nextHeight < minZoomOffset)
            {
                targetHeight = minZoomOffset;
            }
            else if (zoomInput < 0 && nextHeight > maxZoomOffset)
            {
                targetHeight = maxZoomOffset;
            }

            if (targetHeight >= 0f)
            {
                float deltaHeight = nextHeight - currentHeight;
                if (Mathf.Abs(deltaHeight) > 0.001f)
                {
                    t = (targetHeight - currentHeight) / deltaHeight;
                }
            }

            t = Mathf.Clamp01(t);
            transform.position = Vector3.Lerp(currentPos, nextPos, t);
        }

        private Vector3 FoundGroundProjection(Vector3 currentPosition)
        {
            if (Physics.Raycast(currentPosition, Vector3.down, out RaycastHit hit, 1000f, groundLayer,
                    QueryTriggerInteraction.Ignore))
            {
                return hit.point;
            }

            return new Vector3(currentPosition.x, 0, currentPosition.z);
        }

        private void ApplyRotation(float deltaTime)
        {
            float keyboardRotation = controls.CameraControl.RotateKeyboard.ReadValue<float>();

            bool mouseRotationActive = controls.CameraControl.RotateMouseButton.IsPressed();

            float yawDegrees = keyboardRotation * keyboardRotationSpeed * deltaTime;

            if (mouseRotationActive)
            {
                yawDegrees += pendingMouseDelta.x * mouseRotationSensitivity;
            }

            if (Mathf.Abs(yawDegrees) < MinInputMagnitude)
            {
                return;
            }

            transform.rotation = Quaternion.AngleAxis(yawDegrees, Vector3.up) * transform.rotation;
        }

        private void ApplyMovement(float deltaTime)
        {
            Vector2 moveInput = controls.CameraControl.Move.ReadValue<Vector2>();
            bool mouseRotationActive = controls.CameraControl.RotateMouseButton.IsPressed();

            if (!disableEdgeScrollWhileMouseRotating || !mouseRotationActive)
            {
                // moveInput += GetEdgeScrollInput(); todo rollback
            }

            moveInput = Vector2.ClampMagnitude(moveInput, MaxInputMagnitude);

            if (moveInput.sqrMagnitude < MinInputMagnitude) return;

            Vector3 forward = GetPlanarDirection(transform.forward);
            Vector3 right = GetPlanarDirection(transform.right);

            Vector3 displacement = (forward * moveInput.y + right * moveInput.x) * (moveSpeed * deltaTime);
            transform.position += displacement;
        }

        private Vector3 GetPlanarDirection(Vector3 direction)
        {
            direction.y = 0f;

            if (direction.sqrMagnitude < MinInputMagnitude)
            {
                return Vector3.zero;
            }

            return direction.normalized;
        }

        private Vector2 GetEdgeScrollInput()
        {
            Vector2 pointerPosition = controls.CameraControl.PointerPosition.ReadValue<Vector2>();

            if (pointerPosition.x < 0f ||
                pointerPosition.y < 0f ||
                pointerPosition.x > Screen.width ||
                pointerPosition.y > Screen.height)
            {
                return Vector2.zero;
            }

            Vector2 edgeInput = Vector2.zero;

            if (pointerPosition.x <= edgeThresholdPixels)
            {
                edgeInput.x = -1f;
            }
            else if (pointerPosition.x >= Screen.width - edgeThresholdPixels)
            {
                edgeInput.x = 1f;
            }

            if (pointerPosition.y <= edgeThresholdPixels)
            {
                edgeInput.y = -1f;
            }
            else if (pointerPosition.y >= Screen.height - edgeThresholdPixels)
            {
                edgeInput.y = 1f;
            }

            return edgeInput;
        }

        private void InitializeInput()
        {
            if (controls != null)
            {
                return;
            }

            controls = new CameraControls();

            controls.CameraControl.Zoom.performed += OnZoomPerformed;
            controls.CameraControl.RotateMouseDelta.performed += OnRotateMouseDeltaPerformed;
        }

        private void OnZoomPerformed(InputAction.CallbackContext context)
        {
            pendingZoomInput += context.ReadValue<float>();
        }

        private void OnRotateMouseDeltaPerformed(InputAction.CallbackContext context)
        {
            pendingMouseDelta += context.ReadValue<Vector2>();
        }
    }
}