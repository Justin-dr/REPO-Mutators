using System.Collections.Generic;
using Mutators.Mutators;

namespace Mutators.Services.Selection
{
    /// <summary>
    /// Represents a service used to select a weighted mutator from a collection of mutators.
    /// </summary>
    public interface IMutatorSelectionService
    {
        /// <summary>
        /// Selects a weighted mutator from a collection of mutators.
        /// </summary>
        /// <param name="mutators">The mutators that can be picked from.</param>
        /// <returns>The picked <see cref="IMutator"/>.</returns>
        public IMutator GetWeightedMutator(ICollection<IMutator> mutators);
    }
}