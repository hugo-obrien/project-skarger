using _Project.Scripts.Combat;

namespace _Project.Scripts.Units.State
{
    public class UnitIdleState : UnitState
    {
        public UnitIdleState(Unit unit, UnitCombat combat) : base(unit, combat) { }

        public override void Enter()
        {
            unit.Stop();
            unit.SetCombatMode(AttackType.None);
        }
    }
}