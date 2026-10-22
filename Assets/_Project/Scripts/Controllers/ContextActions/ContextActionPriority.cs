namespace _Project.Scripts.Controllers.ContextActions {
    /// <summary>
    /// Well-known priority values for context actions. The lower the value, the earlier
    /// the action is considered when several actions can handle the same context.
    /// </summary>
    public enum ContextActionPriority {
        Highest = 0,
        High = 10,
        Normal = 50,
        Low = 100,
        Fallback = int.MaxValue
    }
}
