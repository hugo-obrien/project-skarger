using _Project.Scripts.Combat;
using _Project.Scripts.Units.Components;

namespace _Project.Scripts.Units.State
{
    public class UnitMoveState : UnitState
    {
        public UnitMoveState(Unit unit, UnitCombat combat) : base(unit, combat) { }

        public override void Enter()
        {
            unit.SetCombatMode(AttackType.None);
        }

        public override void Tick(float deltaTime)
        {
            if (unit.HasReachedDestination())
            {
                combat.StopCombat();
            }
        }
    }
}