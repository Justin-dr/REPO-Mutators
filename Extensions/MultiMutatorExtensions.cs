using Mutators.Mutators;
using System.Collections.Generic;
using System.Linq;

namespace Mutators.Extensions
{
    internal static class MultiMutatorExtensions
    {
        internal static (IList<string> mutators, IDictionary<string, object> meta) Format(this IMultiMutator multiMutator)
        {
            IReadOnlyCollection<IMutator> subMutators = multiMutator.SubMutators;

            IDictionary<string, object> metadata = subMutators.Select(mutator => mutator.Settings.AsMetadata())
                .SelectMany(dict => dict)
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            return (subMutators.Select(mutator => mutator.Name).ToList(), metadata);
        }
    }
}
