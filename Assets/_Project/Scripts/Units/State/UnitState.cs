using _Project.Scripts.Combat;

namespace _Project.Scripts.Units.State
{
    public abstract class UnitState
    {
        protected readonly Unit unit;
        protected readonly UnitCombat combat;

        protected UnitState(Unit unit, UnitCombat combat)
        {
            this.unit = unit;
            this.combat = combat;
        }

        public virtual void Enter() {}
        public virtual void Tick(float deltaTime) {}
        public virtual void Exit() {}
    }
}