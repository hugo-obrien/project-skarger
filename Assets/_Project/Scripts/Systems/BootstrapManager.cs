using System.Collections;
using _Project.Scripts.UI.Console;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapManager : MonoBehaviour
{
    [Header("Load settings")] 
    [Tooltip("Name of next scene to load (must be in BuildSettings)")] [SerializeField] 
    private string nextSceneName = ""; // todo fix it when the main menu is ready
    
    [Header("Global prefabs")]
    [SerializeField] private GameObject debugConsolePrefab;

    private void Awake()
    {
        InitializeGlobalSystems();
        StartCoroutine(LoadNextSceneAsync());
    }

    /// <summary>
    /// Will use for loading global systems
    /// </summary>
    private void InitializeGlobalSystems()
    {
        Debug.Log("[Bootstrap] Global systems initialized.");
    }

    private IEnumerator LoadNextSceneAsync()
    {
        yield return null;
        
        Debug.Log($"[Bootstrap] Start loading scene {nextSceneName}");

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneName);
        while (!asyncLoad.isDone)
        {
            // todo add loading progress bar
            yield return null;
        }
        
        Debug.Log("[Bootstrap] Loading finished");
    }
}
