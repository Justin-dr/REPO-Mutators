using System;
using System.Collections.Generic;
using Mutators.Mutators;
using Mutators.Rules.Loaders.Json;

namespace Mutators.Rules.Loaders.Strategies
{
    /// <summary>
    /// Strategy for loading a specific type of <see cref="JsonMutatorRule"/> for generated <see cref="IMultiMutator"/>s.
    /// </summary>
    public interface IMultiMutatorRuleLoadingStrategy : IRuleLoadingStrategy<Func<IReadOnlyCollection<string>, string, bool>>
    {

    }
}