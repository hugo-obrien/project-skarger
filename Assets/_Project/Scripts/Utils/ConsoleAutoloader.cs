using _Project.Scripts.UI.Console;
using UnityEngine;

namespace _Project.Scripts.Utils
{
    public class ConsoleAutoloader
    {
        private const string CONSOLE_PREFAB_PATH = "Prefabs/DebugConsoleManager";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (Object.FindAnyObjectByType<DebugConsole>() != null)
            {
                LogUtil.Info("ConsoleAutoLoader", "Initialize","Console already exists");
                return;
            }

            GameObject consolePrefab = Resources.Load<GameObject>(CONSOLE_PREFAB_PATH);
            if (consolePrefab == null)
            {
                LogUtil.Error("ConsoleAutoLoader", "Initialize", "Unable to load console prefab");
                return;
            }

            GameObject consoleInstance = Object.Instantiate(consolePrefab);
            consoleInstance.name = "DebugConsole_AutoInstance";
        }
    }
}