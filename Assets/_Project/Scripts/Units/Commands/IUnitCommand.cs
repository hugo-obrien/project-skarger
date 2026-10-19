namespace _Project.Scripts.Units.Commands
{
    public interface IUnitCommand
    {
        bool CanStart { get; }
        bool IsCompleted { get; }

        void Begin();
        void Tick(float deltaTime);
        void Cancel();
    }
}