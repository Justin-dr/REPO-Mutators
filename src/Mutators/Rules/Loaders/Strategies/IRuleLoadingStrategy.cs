using System.Collections.Generic;
using Mutators.Rules.Loaders.Json;

namespace Mutators.Rules.Loaders.Strategies
{
    /// <summary>
    /// Strategy for loading a specific type of <see cref="JsonMutatorRule"/>.
    /// </summary>
    /// <typeparam name="T">The type of the executable rule.</typeparam>
    public interface IRuleLoadingStrategy<T>
    {
        /// <summary>
        /// The unique identifier of the rule.
        /// </summary>
        string Key { get; }
        
        /// <summary>
        /// Validate the rule. If it is invalid, the rule will not be loaded.
        /// </summary>
        /// <param name="rule">The rule to be validated.</param>
        /// <returns>True if valid, otherwise false.</returns>
        bool Validate(JsonMutatorRule rule);
        
        /// <summary>
        /// Load the rule.
        /// </summary>
        /// <param name="mutators">The rule's mutators.</param>
        /// <param name="arguments">The rule's additional arguments.</param>
        /// <returns>The executable rule.</returns>
        T Load(IReadOnlyList<string> mutators, IReadOnlyDictionary<string, object>? arguments = null);
    }
}