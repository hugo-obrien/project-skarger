namespace _Project.Scripts.Controllers.ContextActions {
    /// <summary>
    /// A strategy that describes what a right click does in a specific context.
    /// Implement this interface to introduce new right-click behavior.
    /// </summary>
    public interface IContextAction {
        /// <summary>
        /// The lower the value, the earlier the action is considered.
        /// </summary>
        int Priority { get; }

        bool CanHandle(RightClickContext context);

        void Execute(RightClickContext context, RightClickContextServices services);
    }
}
