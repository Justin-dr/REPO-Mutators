using System;
using System.Collections.Generic;
using Mutators.Rules.Loaders.Json;
using static Mutators.Rules.MultiMutatorRules;

namespace Mutators.Rules.Loaders.Strategies
{
    /// <summary>
    /// Default rule loading strategy for <see cref="MultiMutatorRuleType.MutualExclusion"/>.
    /// </summary>
    public class MutualExclusionRuleLoadingStrategy : IMultiMutatorRuleLoadingStrategy
    {
        /// <summary>
        /// <inheritdoc cref="IRuleLoadingStrategy{T}.Key"/>
        /// </summary>
        public string Key => MultiMutatorRuleType.MutualExclusion;

        /// <summary>
        /// <inheritdoc cref="IRuleLoadingStrategy{T}.Validate"/>
        /// <para>
        /// For this rule to be valid, the mutator list must contain exactly two mutators.
        /// </para>
        /// </summary>
        /// <param name="rule">The rule to be validated</param>
        /// <returns><inheritdoc cref="IRuleLoadingStrategy{T}.Validate"/></returns>
        public bool Validate(JsonMutatorRule rule)
        {
            if (rule.Mutators.Count != 2)
            {
                RepoMutators.Logger.LogError($"Skipping invalid rule {rule.Key}: mutual exclusion rules must have exactly 2 mutators");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Load the mutual exclusion rule.
        /// </summary>
        /// <param name="mutators">The mutators that will be mutually exclusive.</param>
        /// <param name="arguments">The rule's additional arguments, ignored in this implementation.</param>
        /// <returns>The executable mutual exclusion rule.</returns>
        public Func<IReadOnlyCollection<string>, string, bool> Load(IReadOnlyList<string> mutators, IReadOnlyDictionary<string, object>? arguments)
        {
            return MutualExclusionRule(mutators[0], mutators[1]);
        }
    }
}