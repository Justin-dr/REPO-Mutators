using System;
using System.Linq;
using Mutators.Mutators;

namespace Mutators.Rules.Registries
{
    /// <summary>
    /// Registry for <see cref="IMutator"/> selection rules.
    /// <remarks>
    /// Only applies to <see cref="IMutator"/>s that are not <see cref="IMultiMutator"/>s.
    /// </remarks>
    /// </summary>
    public class SingleMutatorSelectionRulesRegistry : MutatorSelectionRulesRegistry<Predicate<string>, Predicate<IMutator>>
    {
        internal bool RunRules(IMutator mutator)
        {
            return mutatorSelectionRulesExtended.Values.All(rule => rule(mutator)) &&
                   mutatorSelectionRules.Values.All(rule => rule(mutator.NamespacedName));
        }
    }
}