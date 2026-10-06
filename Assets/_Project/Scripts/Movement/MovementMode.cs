namespace _Project.Scripts.Movement
{
    /// <summary>
    /// Unit movement speed mode: automatic selection (walking/running based on distance) or manual override.
    /// </summary>
    public enum MovementMode
    {
        Auto,
        ForcedWalk,
        ForcedRun
    }
}