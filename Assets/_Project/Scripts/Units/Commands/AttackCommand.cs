using _Project.Scripts.Units.Components;

namespace _Project.Scripts.Units.Commands
{
    public sealed class AttackCommand : IUnitCommand
    {
        private readonly UnitCombat combat;
        private readonly Unit target;

        private bool started;

        public AttackCommand(UnitCombat combat, Unit target)
        {
            this.combat = combat;
            this.target = target;
        }

        public bool CanStart => target != null && !target.IsDead;

        public bool IsCompleted => started && !combat.HasValidTarget;

        public void Begin()
        {
            started = true;
            combat.Attack(target);
        }

        public void Tick(float deltaTime)
        {
        }

        public void Cancel()
        {
            if (started)
            {
                combat.StopCombat();
            }
        }
    }
}