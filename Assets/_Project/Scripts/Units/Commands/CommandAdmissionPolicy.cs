namespace _Project.Scripts.Units.Commands
{
    public static class CommandAdmissionPolicy
    {
        public static CommandAdmission Evaluate(
            bool queueIsEmpty,
            bool queueHasPlayerCommands,
            CommandSource source,
            bool appendRequested)
        {
            if (source == CommandSource.AI)
            {
                return queueIsEmpty ? CommandAdmission.Append : CommandAdmission.Reject;
            }

            if (queueIsEmpty || !queueHasPlayerCommands)
            {
                return CommandAdmission.Replace;
            }

            return appendRequested ? CommandAdmission.Append : CommandAdmission.Replace;
        }
    }
}