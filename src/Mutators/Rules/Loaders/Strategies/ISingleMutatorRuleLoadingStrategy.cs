using System;
using Mutators.Mutators;
using Mutators.Rules.Loaders.Json;

namespace Mutators.Rules.Loaders.Strategies
{
    /// <summary>
    /// Strategy for loading a specific type of <see cref="JsonMutatorRule"/> for <see cref="IMutator"/>s.
    /// </summary>
    public interface ISingleMutatorRuleLoadingStrategy : IRuleLoadingStrategy<Predicate<string>>
    {
        
    }
}