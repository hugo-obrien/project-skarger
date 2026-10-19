using System.Collections.Generic;

namespace _Project.Scripts.Units.Commands
{
    public sealed class UnitCommandQueue
    {
        private readonly List<Entry> entries = new();

        public bool IsEmpty => entries.Count == 0;

        public bool HasPlayerCommands
        {
            get
            {
                foreach (var entry in entries)
                {
                    if (entry.Source == CommandSource.Player) return true;
                }

                return false;
            }
        }

        public void Enqueue(IUnitCommand command, CommandSource source)
        {
            entries.Add(new Entry(command, source));

            if (entries.Count == 1)
            {
                StartHead();
            }
        }

        public void Clear()
        {
            if (!IsEmpty)
            {
                entries[0].Command.Cancel();
            }

            entries.Clear();
        }

        public void Tick(float deltaTime)
        {
            if (IsEmpty) return;

            entries[0].Command.Tick(deltaTime);

            while (!IsEmpty && entries[0].Command.IsCompleted)
            {
                entries.RemoveAt(0);

                StartHead();
            }
        }

        private void StartHead()
        {
            while (!IsEmpty)
            {
                var head = entries[0].Command;
                if (head.CanStart)
                {
                    head.Begin();
                    return;
                }

                entries.RemoveAt(0);
            }
        }

        private readonly struct Entry
        {
            public readonly IUnitCommand Command;
            public readonly CommandSource Source;

            public Entry(IUnitCommand command, CommandSource source)
            {
                Command = command;
                Source = source;
            }
        }
    }
}