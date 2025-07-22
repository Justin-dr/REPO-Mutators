using System;
using System.Collections.Generic;
using Mutators.Rules.Loaders.Json;

namespace Mutators.Rules.Loaders.Strategies
{
    /// <summary>
    /// Rule loading strategy for <see cref="MultiMutatorRuleType.RequiresAmountOfOtherMutators"/>.
    /// </summary>
    public class RequiresAmountOfOtherMutatorsRuleLoadingStrategy : IMultiMutatorRuleLoadingStrategy
    {
        /// <summary>
        /// <inheritdoc cref="IRuleLoadingStrategy{T}.Key"/>
        /// </summary>
        public string Key => MultiMutatorRuleType.RequiresAmountOfOtherMutators;

        /// <summary>
        /// <inheritdoc cref="IRuleLoadingStrategy{T}.Validate"/>
        /// <para>
        /// For this rule to be valid, the mutator list must contain exactly one mutator.
        /// </para>
        /// </summary>
        /// <param name="rule">The rule to be validated.</param>
        /// <returns><inheritdoc cref="IRuleLoadingStrategy{T}.Validate"/></returns>
        public bool Validate(JsonMutatorRule rule)
        {
            if (rule.Mutators.Count != 1)
            {
                return false;
            }
            
            if (rule.Arguments == null || !rule.Arguments.TryGetValue("other-mutators-required-amount", out object? mutatorsRequiredObject))
            {
                return false;
            }

            if (mutatorsRequiredObject is not long)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Load the required amount of other mutators rule.
        /// </summary>
        /// <param name="mutators">The mutator that requires other mutators.</param>
        /// <param name="arguments">The rule's additional arguments, ignored in this implementation.</param>
        /// <returns>The executable required amount of other mutators rule.</returns>
        public Func<IReadOnlyCollection<string>, string, bool> Load(IReadOnlyList<string> mutators, IReadOnlyDictionary<string, object>? arguments = null)
        {
            long? argument = arguments!["other-mutators-required-amount"] as long?;
            
            // This is validated in Validate, so we'll ignore the null warning here
            return MultiMutatorRules.RequiresAmountOfOtherMutatorsRule(mutators[0], (int)Math.Clamp(argument!.Value, 0, 10));
        }
    }
}