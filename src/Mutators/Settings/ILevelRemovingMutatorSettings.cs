using System.Collections.Generic;

namespace Mutators.Settings
{
    /// <summary>
    /// Settings for mutators that remove levels.
    /// </summary>
    internal interface ILevelRemovingMutatorSettings
    {
        /// <summary>
        /// Whether custom levels are allowed.
        /// </summary>
        bool AllowCustomLevels { get; }
        /// <summary>
        /// List of level names that should be excluded when the mutator is active.
        /// </summary>
        IList<string> ExcludedLevels { get; }
    }
}
