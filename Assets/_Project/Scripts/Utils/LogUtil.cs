using UnityEngine;

namespace _Project.Scripts.Utils {
    public static class LogUtil {

        public static void Info(string className, string methodName, string message) {
            Log(LogType.Log, className, methodName, message);
        }
        
        public static void Warn(string className, string methodName, string message) {
            Log(LogType.Warning, className, methodName, message);
        }
        
        public static void Error(string className, string methodName, string message) {
            Log(LogType.Error, className, methodName, message);
        }

        public static void Log(LogType type, string className, string methodName, string message) {
            string formatted = $"{className}.{methodName}(): {message}";
            Debug.unityLogger.Log(type, formatted);
        }
    }
}