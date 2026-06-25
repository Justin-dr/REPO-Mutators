using System.Collections.Generic;
using Mutators.Rules.Loaders.Strategies;
using Newtonsoft.Json;

namespace Mutators.Rules.Loaders.Json
{
    /// <summary>
    /// Represents a mutator rule in a JSON file.
    /// </summary>
    public class JsonMutatorRule
    {
        /// <summary>
        /// The unique identifier of the rule.
        /// </summary>
        public string Key { get; }
        
        /// <summary>
        /// The type of the rule.
        /// <remarks>
        /// An appropriate <see cref="IRuleLoadingStrategy{T}"/> needs to be registered for this type for it to be loaded.
        /// </remarks>
        /// </summary>
        public string Type { get; }
        
        /// <summary>
        /// The mutators to which the rule applies.
        /// </summary>
        public IReadOnlyList<string> Mutators { get; }
        
        /// <summary>
        /// Optional arguments for the rule.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyDictionary<string, object>? Arguments { get; }
        
        /// <summary>
        /// Whether the rule is enabled.
        /// </summary>
        public bool Enabled { get; }
        
        [JsonConstructor]
        public JsonMutatorRule(string key, string type, string[]? mutators = null, string? mutator = null, IDictionary<string, object>? arguments = null, bool enabled = true)
        {
            Key = key;
            Type = type;
            Mutators = mutators ?? (mutator is null ? [] : [mutator]);
            Arguments = arguments != null ? new Dictionary<string, object>(arguments) : null;
            Enabled = enabled;
        }
    }
}
