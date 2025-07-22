using Mutators.Rules.Loaders.Json;
using Mutators.Rules.Loaders.Strategies;

namespace Mutators.Rules.Loaders
{
    /// <summary>
    /// Loads JSON rule definitions into executable rules.
    /// </summary>
    /// <typeparam name="T">The executable rule type produced by this loader.</typeparam>
    public interface IRuleLoader<T>
    {
        /// <summary>
        /// Adds a default rule written when the rule file does not contain a rule with the same key.
        /// </summary>
        /// <param name="jsonRule">The default JSON rule definition to add.</param>
        public void AddDefaultRule(JsonMutatorRule jsonRule);

        /// <summary>
        /// Adds multiple default rules that are written when the rule file does not contain a rule with the same key.
        /// </summary>
        /// <param name="jsonRules">The default JSON rule definitions to add.</param>
        public void AddDefaultRules(params JsonMutatorRule[] jsonRules);

        /// <summary>
        /// Adds a strategy that maps a JSON rule type to an executable rule.
        /// </summary>
        /// <param name="strategy">The strategy to use for matching JSON rules.</param>
        public void AddRuleStrategy(IRuleLoadingStrategy<T> strategy);
    }
}
