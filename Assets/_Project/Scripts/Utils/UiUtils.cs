using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Scripts.Utils {
    public static class UiUtils {
        public static bool IsPointerOverUI() {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
        
        
    }
}