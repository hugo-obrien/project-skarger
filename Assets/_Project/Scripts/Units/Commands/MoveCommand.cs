using _Project.Scripts.Units.Components;
using UnityEngine;

namespace _Project.Scripts.Units.Commands
{
    public sealed class MoveCommand : IUnitCommand
    {
        private readonly Unit unit;
        private readonly UnitCombat combat;
        private readonly Vector3 destination;

        private bool started;
        private bool failed;

        public MoveCommand(Unit unit, UnitCombat combat, Vector3 destination)
        {
            this.unit = unit;
            this.combat = combat;
            this.destination = destination;
        }

        public bool CanStart => true;

        public bool IsCompleted => failed || (started && unit.HasReachedDestination());

        public void Begin()
        {
            started = true;
            combat.StopCombat();
            failed = !unit.MoveTo(destination);
        }

        public void Tick(float deltaTime)
        {
        }

        public void Cancel()
        {
            unit.Stop();
        }
    }
}