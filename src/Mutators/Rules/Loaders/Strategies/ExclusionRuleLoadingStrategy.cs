using System;
using System.Collections.Generic;
using Mutators.Rules.Loaders.Json;

namespace Mutators.Rules.Loaders.Strategies
{
    /// <summary>
    /// Default rule loading strategy for <see cref="SingleMutatorRuleType.Exclusion"/>.
    /// </summary>
    public class ExclusionRuleLoadingStrategy : ISingleMutatorRuleLoadingStrategy
    {
        /// <summary>
        /// <inheritdoc cref="IRuleLoadingStrategy{T}.Key"/>
        /// </summary>
        public string Key => SingleMutatorRuleType.Exclusion;
        
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
            return rule.Mutators.Count == 1 && !string.IsNullOrEmpty(rule.Mutators[0]);
        }

        /// <summary>
        /// Load the exclusion rule.
        /// </summary>
        /// <param name="mutators">The mutator that will be excluded.</param>
        /// <param name="arguments">The rule's additional arguments, ignored in this implementation.</param>
        /// <returns>The executable exclusion rule.</returns>
        public Predicate<string> Load(IReadOnlyList<string> mutators, IReadOnlyDictionary<string, object>? arguments = null)
        {
            return mutator => mutator != mutators[0];
        }
    }
    
    /// <summary>
    /// Default rule loading strategy for <see cref="MultiMutatorRuleType.Exclusion"/>.
    /// </summary>
    public class MultiExclusionRuleLoadingStrategy : IMultiMutatorRuleLoadingStrategy
    {
        private readonly ExclusionRuleLoadingStrategy _exclusionRuleLoadingStrategy = new();
        /// <summary>
        /// <inheritdoc cref="IRuleLoadingStrategy{T}.Key"/>
        /// </summary>
        public string Key => _exclusionRuleLoadingStrategy.Key;
        
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
            return _exclusionRuleLoadingStrategy.Validate(rule);
        }

        /// <summary>
        /// Load the exclusion rule.
        /// </summary>
        /// <param name="mutators">The mutator that will be excluded.</param>
        /// <param name="arguments">The rule's additional arguments, ignored in this implementation.</param>
        /// <returns>The executable exclusion rule.</returns>
        public Func<IReadOnlyCollection<string>, string, bool> Load(IReadOnlyList<string> mutators, IReadOnlyDictionary<string, object>? arguments = null)
        {
            Predicate<string> exclusionRule = _exclusionRuleLoadingStrategy.Load(mutators, arguments);
            return (_, mutator) => exclusionRule(mutator);
        }
    }
}