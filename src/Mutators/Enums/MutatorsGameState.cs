namespace Mutators.Enums
{
    /// <summary>
    /// Represents the current game phase relevant to mutator selection, activation, and metadata application.
    /// </summary>
    public enum MutatorsGameState
    {
        /// <summary>
        /// The game is not in a state that requires mutators.
        /// </summary>
        None = 0,
        /// <summary>
        /// The game is in the shopping phase.
        /// </summary>
        Shop = 5,
        /// <summary>
        /// The level setup is done, but the level hasn't started yet.
        /// <remarks>
        /// The LevelGenerator's <c>LevelState</c> is already set to <c>Done</c> at this point, yet its <c>Generated</c> flag is still false.
        /// </remarks>
        /// </summary>
        LevelReady = 29,
        /// <summary>
        /// The level setup is done, the level is marked as generated, and the level has started.
        /// </summary>
        LevelGenerated = 30
    }
}
