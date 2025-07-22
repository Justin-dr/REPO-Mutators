using System;
using System.Collections.Generic;
using System.IO;
using Mutators.Rules.Loaders.Json;

namespace Mutators.Rules.Loaders
{
    internal sealed class MultiMutatorRuleLoader : RuleLoader<Func<IReadOnlyCollection<string>, string, bool>>
    {
        private static readonly string MultiMutatorRulesPath = Path.Combine(RepoMutators.ConfigPath, "multi-mutator-rules.json");

        protected override JsonMutatorRule[] CreateDefaultRules()
        {
            return [];
        }

        protected override string GetRulesPath()
        {
            return MultiMutatorRulesPath;
        }
    }
}
