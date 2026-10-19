using _Project.Scripts.Units;

namespace _Project.Scripts.Units.Commands
{
    public sealed class SpeechBubbleCommand : IUnitCommand
    {
        private readonly Unit speaker;
        private readonly string text;

        private bool started;

        public SpeechBubbleCommand(Unit speaker, string text)
        {
            this.speaker = speaker;
            this.text = text;
        }

        public bool CanStart => speaker != null && !speaker.IsDead;

        public bool IsCompleted => started;

        public void Begin()
        {
            started = true;
            speaker.ShowSpeechBubble(text);
        }

        public void Tick(float deltaTime)
        {
        }

        public void Cancel()
        {
        }
    }
}