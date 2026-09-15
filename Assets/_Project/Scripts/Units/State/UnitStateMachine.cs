using UnityEngine;

namespace _Project.Scripts.Units.State
{
    public class UnitStateMachine
    {
        public UnitState CurrentState { get; private set; }

        public void ChangeState(UnitState newState)
        {
            Debug.Log($"{newState.unit.name} move to {newState}");
            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState?.Enter();
        }

        public void Tick(float deltaTime)
        {
            CurrentState?.Tick(deltaTime);
        }

        public void Stop()
        {
            CurrentState?.Exit();
            CurrentState = null;
        }
    }
}