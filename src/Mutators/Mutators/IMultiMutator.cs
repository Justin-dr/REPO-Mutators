using System.Collections.Generic;
using Mutators.Managers;

namespace Mutators.Mutators
{
    /// <summary>
    /// Represents a multi-mutator that can be registered, selected, activated, and deactivated by the <see cref="MutatorManager"/>.
    /// </summary>
    public interface IMultiMutator : IMutator
    {
        /// <summary>
        /// A dictionary with the sub-mutators of this multi-mutator as the keys, and their settings as the values.
        /// </summary>
        IReadOnlyDictionary<IMutator, IDictionary<string, object>> SubMutators { get; }
    }
}