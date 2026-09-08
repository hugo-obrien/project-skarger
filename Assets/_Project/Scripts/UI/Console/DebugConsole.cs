using System;
using System.Collections.Generic;
using _Project.Scripts.Units;
using _Project.Scripts.Utils;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.UI.Console {
    public class DebugConsole : MonoBehaviour {
        private SelectionGroup cachedSelectionGroup;
        
        [Header("References")]
        [SerializeField] private ConsoleLogItemUI consoleLogItemUI;
        [SerializeField] private TMP_InputField inputField;

        [Header("Settings")] 
        [SerializeField] private KeyCode toggleKey = KeyCode.BackQuote;
        [SerializeField] private float defaultDamage = 50f;
        [SerializeField] private GameObject consolePanel;

        private Dictionary<string, IConsoleCommand> commands;
        private bool isOpen;
        private bool isInitialized;
        
        public IReadOnlyCollection<IConsoleCommand> Commands => commands.Values;
        public bool IsOpen => isOpen;

        public SelectionGroup ActiveSelectionGroup
        {
            get
            {
                if (cachedSelectionGroup == null)
                {
                    cachedSelectionGroup = FindAnyObjectByType<SelectionGroup>();
                    if (cachedSelectionGroup == null)
                    {
                        LogToConsole("Selection group not found at scene");
                    }
                }

                return cachedSelectionGroup;
            }
            set => cachedSelectionGroup = value;
        }

        private void Awake() {
            DontDestroyOnLoad(gameObject);
            Initialize();
        }

        private void Update() {
            if (Input.GetKeyDown(toggleKey)) {
                ToggleConsole();
            }

            if (isOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleConsole();
            }
        }
        
        private void OnDestroy()
        {
            inputField.onEndEdit.RemoveListener(OnInputSubmitted);
        }
        
        private void Initialize()
        {
            if (isInitialized)
            {
                return;
            }
            
            commands = new Dictionary<string, IConsoleCommand>(StringComparer.OrdinalIgnoreCase);
            
            RegisterCommand(new DamageCommand(this, defaultDamage));
            RegisterCommand(new SayCommand(this));
            RegisterCommand(new HelpCommand(this));

            isInitialized = true;
            
            inputField.onSubmit.AddListener(OnInputSubmitted);
            
            consolePanel.SetActive(false);
        }

        private void ToggleConsole()
        {
            isOpen = !isOpen;
            consolePanel.SetActive(isOpen);

            if (isOpen)
            {
                inputField.ActivateInputField();
                inputField.Select();
            }
            else
            {
                inputField.DeactivateInputField();
            }
        }

        public void RegisterCommand(IConsoleCommand command) {
            if (string.IsNullOrEmpty(command.Name)) return;
            
            commands[command.Name] = command;
            LogUtil.Info("Console", "RegisterCommand", $"Command {command.Name} registered");
        }
        
        public void LogToConsole(string message, Color? color = null)
        {
            consoleLogItemUI?.AddLogEntry(message, color);
            if (color == Color.red)
            {
                Debug.LogError(message);
            }
            else if (color == Color.yellow)
            {
                Debug.LogWarning(message);
            }
            else
            {
                Debug.Log(message);
            }
        }
        
        private void OnInputSubmitted(string text)
        {
            if (isOpen)
            {
                ExecuteCommand(text);
            }
        }

        private void ExecuteCommand(string input) {
            if (string.IsNullOrWhiteSpace(input)) return;
            
            string[] parts = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string commandName = parts[0];
            string[] args = parts.Length > 1 ? parts[1..] : Array.Empty<string>();

            if (commands.TryGetValue(commandName, out IConsoleCommand command))
            {
                try
                {
                    command.Execute(args);
                }
                catch (Exception ex)
                {
                    LogToConsole($"Execution error: {ex.Message}", Color.red);
                }
            }
            else
            {
                LogToConsole($"Unknown command: {commandName}. Enter help to get commands");
            }

            inputField.text = "";
        }
    }
}