using System;
using System.Collections.Generic;

namespace Mutators.Rules.Registries
{
    /// <summary>
    /// Abstract base class for SelectionRuleRegistries.
    /// </summary>
    /// <typeparam name="T">The basic rule type.</typeparam>
    /// <typeparam name="TE">The extended rule type.</typeparam>
    public abstract class MutatorSelectionRulesRegistry<T, TE>
    {
        /// <summary>
        /// Dictionary of registered rules.
        /// </summary>
        protected readonly IDictionary<string, T> mutatorSelectionRules = new Dictionary<string, T>();
        
        /// <summary>
        /// Dictionary of registered extended rules.
        /// </summary>
        protected readonly IDictionary<string, TE> mutatorSelectionRulesExtended = new Dictionary<string, TE>();

        /// <summary>
        /// Registers a new rule.
        /// </summary>
        /// <param name="key">The unique identifier of the rule.</param>
        /// <param name="rule">The rule to be registered.</param>
        public void Register(string key, T rule)
        {
            ValidateAndThen(key, () => mutatorSelectionRules.Add(key, rule));
        }

        /// <summary>
        /// Registers a new rule.
        /// </summary>
        /// <param name="key">The unique identifier of the rule.</param>
        /// <param name="rule">The rule to be registered.</param>
        public void Register(string key, TE rule)
        {
            ValidateAndThen(key, () => mutatorSelectionRulesExtended.Add(key, rule));
        }

        /// <summary>
        /// Unregisters a rule.
        /// </summary>
        /// <param name="key">The unique identifier of the rule.</param>
        /// <returns>True if the rule was successfully removed, otherwise false.</returns>
        public bool Unregister(string key)
        {
            return mutatorSelectionRules.Remove(key) || mutatorSelectionRulesExtended.Remove(key);
        }

        /// <summary>
        /// Checks if a rule with the supplied key is registered.
        /// </summary>
        /// <param name="key">The unique identifier of the rule.</param>
        /// <returns>True if there is a registered rule with the supplied key, otherwise false</returns>
        public bool IsRuleRegistered(string key)
        {
            return mutatorSelectionRules.ContainsKey(key) || mutatorSelectionRulesExtended.ContainsKey(key);
        }

        private void ValidateAndThen(string key, Action action)
        {
            if (IsRuleRegistered(key))
            {
                throw new InvalidOperationException($"Rule with key {key} already exists");
            }
            
            action();
        }
    }
}