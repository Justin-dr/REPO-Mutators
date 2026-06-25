namespace Mutators.Enums
{
    /// <summary>
    /// Source of the mutator.
    /// </summary>
    public enum MutatorSource
    {
        /// <summary>
        /// The user added the mutator, likely as a multi-mutator.
        /// </summary>
        User,
        /// <summary>
        /// Mutators or another mod added the mutator. Common for base mutators and generated multi-mutators.
        /// </summary>
        Mod
    }
}