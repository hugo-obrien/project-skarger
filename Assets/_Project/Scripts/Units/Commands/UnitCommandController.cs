using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Units.Commands {
    [RequireComponent(typeof(Unit))]
    public sealed class UnitCommandController : MonoBehaviour {
        private readonly UnitCommandQueue _queue = new();

        private Unit _unit;

        public bool IsIdle => _queue.IsEmpty;

        private void Awake() {
            _unit = GetComponent<Unit>();
        }

        private void OnEnable() {
            _unit.Died += OnUnitDied;
        }

        private void OnDisable() {
            _unit.Died -= OnUnitDied;
        }

        private void Update() {
            _queue.Tick(Time.deltaTime);
        }

        private void OnUnitDied() {
            _queue.Clear();
        }

        public void Issue(IUnitCommand command, CommandSource source, bool appendRequested) {
            CommandAdmission admission = CommandAdmissionPolicy.Evaluate(
                _queue.IsEmpty, _queue.HasPlayerCommands, source, appendRequested);

            switch (admission) {
                case CommandAdmission.Replace: {
                    _queue.Clear();
                    _queue.Enqueue(command, source);
                    break;
                }
                case CommandAdmission.Append:
                    _queue.Enqueue(command, source);
                    break;
                case CommandAdmission.Reject:
                default:
                    break;
            }
        }

        public void IssueSequence(IReadOnlyList<IUnitCommand> commands, CommandSource source, bool appendRequested) {
            CommandAdmission admission = CommandAdmissionPolicy.Evaluate(
                _queue.IsEmpty, _queue.HasPlayerCommands, source, appendRequested);

            switch (admission) {
                case CommandAdmission.Replace:
                    _queue.Clear();
                    EnqueueAll(commands, source);
                    break;
                case CommandAdmission.Append:
                    EnqueueAll(commands, source);
                    break;
                case CommandAdmission.Reject:
                default:
                    break;
            }
        }

        private void EnqueueAll(IReadOnlyList<IUnitCommand> commands, CommandSource source) {
            foreach (var command in commands) {
                _queue.Enqueue(command, source);
            }
        }
    }
}