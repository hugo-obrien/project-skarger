using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace _Project.Scripts.Utils {
    public static class UiUtils {
        private static readonly List<RaycastResult> s_RaycastResults = new List<RaycastResult>();

        public static bool IsPointerOverUI() {
            // New Input System: query the active EventSystem raycasters directly with the
            // current pointer position instead of legacy Input.mousePosition-based checks.
            if (EventSystem.current == null) return false;

            var pointer = Pointer.current;
            if (pointer == null) return false;

            var eventData = new PointerEventData(EventSystem.current)
            {
                position = pointer.position.ReadValue()
            };

            s_RaycastResults.Clear();
            EventSystem.current.RaycastAll(eventData, s_RaycastResults);
            return s_RaycastResults.Count > 0;
        }

        public static bool IsShiftPressed() {
            var keyboard = Keyboard.current;
            if (keyboard == null) return false;
            return keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
        }
    }
}
