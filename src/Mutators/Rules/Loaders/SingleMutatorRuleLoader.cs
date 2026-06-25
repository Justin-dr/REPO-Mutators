using System;
using System.IO;
using System.Linq;
using Mutators.Rules.Loaders.Json;
using Mutators.Rules.Registries;

namespace Mutators.Rules.Loaders
{
    internal sealed class SingleMutatorRuleLoader : RuleLoader<Predicate<string>>
    {
        private static readonly string SingleMutatorRulesPath = Path.Combine(RepoMutators.ConfigPath, "single-mutator-rules.json");

        protected override JsonMutatorRule[] CreateDefaultRules()
        {
            return [];
        }

        protected override string GetRulesPath()
        {
            return SingleMutatorRulesPath;
        }
        
        private void ValidateAndRegisterExclusionRule(SingleMutatorSelectionRulesRegistry registry, JsonMutatorRule rule)
        {
            if (string.IsNullOrWhiteSpace(rule.Key))
            {
                RepoMutators.Logger.LogError("Skipping invalid rule: exclusion rules must have a non-empty key");
                return;
            }

            if (rule.Mutators.Any(string.IsNullOrWhiteSpace))
            {
                RepoMutators.Logger.LogError($"Skipping invalid rule {rule.Key}: exclusion rules must have non-empty mutators");
                return;
            }

            if (rule.Mutators.Count != 1)
            {
                RepoMutators.Logger.LogError($"Skipping invalid rule {rule.Key}: exclusion rules must have exactly 1 mutators");
                return;
            }

            registry.Register(rule.Key, SingleMutatorRules.ExclusionRule(rule.Mutators[0]));
        }
    }
}