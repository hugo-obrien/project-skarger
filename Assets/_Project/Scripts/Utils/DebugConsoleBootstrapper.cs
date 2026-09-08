using System;
using _Project.Scripts.UI.Console;
using UnityEngine;

namespace _Project.Scripts.Utils
{
    public class DebugConsoleBootstrapper : MonoBehaviour
    {
        [SerializeField] private GameObject debugConsolePrefab;

        private void Awake()
        {
            if (FindAnyObjectByType<DebugConsole>() == null)
            {
                Instantiate(debugConsolePrefab);
            }
            
            Destroy(gameObject);
        }
    }
}