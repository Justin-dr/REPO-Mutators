using System;
using Mutators.Mutators;
using Mutators.Rules.Registries;

namespace Mutators.Rules
{
    /// <summary>
    /// Premade rules for <see cref="IMutator"/>s.
    /// </summary>
    public static class SingleMutatorRules
    {
        /// <summary>
        /// A rule that excludes a mutator by <see cref="IMutator.NamespacedName">NamespacedName</see>.
        /// </summary>
        /// <param name="namespacedName"></param>
        /// <returns>A predicate to be evaluated by the <see cref="SingleMutatorSelectionRulesRegistry"/></returns>
        public static Predicate<IMutator> ExclusionRule(string namespacedName)
        {
            return mutator => mutator.NamespacedName != namespacedName;
        }
    }
}