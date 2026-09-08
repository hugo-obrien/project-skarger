using _Project.Scripts.Units;
using _Project.Scripts.Utils;
using UnityEngine;

namespace _Project.Scripts.UI.Console {
    public interface IConsoleCommand {
        string Name { get; }
        string Description { get; }
        void Execute(string[] args);
    }
    
    public class HelpCommand : IConsoleCommand
    {
        private readonly DebugConsole console;

        public HelpCommand(DebugConsole console)
        {
            this.console = console;
        }

        public string Name => "help";
        public string Description => "List of available commands";

        public void Execute(string[] args)
        {
            console.LogToConsole("=== Available commands ===", Color.cyan);
            foreach (var command in console.Commands)
            {
                console.LogToConsole($"{command.Name} - {command.Description}");
            }
            console.LogToConsole("=== === ===", Color.cyan);
        }
    }
    
    public class DamageCommand : IConsoleCommand {

        private readonly DebugConsole console;
        private readonly float defaultDamage;

        public DamageCommand(DebugConsole console, float defaultDamage = 50f) {
            this.console = console;
            this.defaultDamage = defaultDamage;
        }

        public string Name => "damage";
        public string Description => "Deal the damage to selected units";
        
        public void Execute(string[] args)
        {
            var selectionGroup = console.ActiveSelectionGroup;
            if (selectionGroup.SelectedUnits.Count == 0) {
                LogUtil.Info("DamageCommand", "Execute", "No units selected");
                return;
            }

            float damageAmount = defaultDamage;
            if (args.Length > 0  && float.TryParse(args[0], out float parsedDamage)) {
                damageAmount = parsedDamage;
            }

            int affectedCount = 0;
            foreach (var unit in selectionGroup.SelectedUnits) {
                if (unit != null && !unit.IsDead) {
                    unit.TakeDamage(damageAmount, Vector3.zero);
                    affectedCount++;
                }
            }
            
            console.LogToConsole($"damage {damageAmount}");
            LogUtil.Info("DamageCommand", "Execute", $"Afflicted {damageAmount} to {affectedCount} units");
        }
    }
    
    public class SayCommand : IConsoleCommand {

        private readonly DebugConsole console;
        private readonly string defaultMessage;

        public SayCommand(DebugConsole console, string defaultMessage = "Hello, world!") {
            this.console = console;
            this.defaultMessage = defaultMessage;
        }

        public string Name => "message";
        public string Description => "Make selected units say something";
        
        public void Execute(string[] args)
        {
            var selectionGroup = console.ActiveSelectionGroup;
            if (selectionGroup.SelectedUnits.Count == 0) {
                LogUtil.Info("SayCommand", "Execute", "No units selected");
                return;
            }
            
            var message = args.Length > 0 ? args[0] : defaultMessage;

            foreach (var unit in selectionGroup.SelectedUnits) {
                if (unit != null && !unit.IsDead) {
                    unit.SaySomething(message);
                }
            }
            
            LogUtil.Info("DamageCommand", "Execute", $"Someone say {message}");
        }
    }
}