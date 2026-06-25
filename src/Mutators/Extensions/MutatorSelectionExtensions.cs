using System;
using System.Collections.Generic;
using System.Linq;
using Mutators.Mutators;

namespace Mutators.Extensions
{
    internal static class MutatorSelectionExtensions
    {
        internal static IList<IMutator> GetSelection(this ICollection<IMutator> mutators, Predicate<IMutator> predicate)
        {
            return mutators.Where(mutator => mutator.IsEligibleForSelection() && predicate(mutator)).ToList();
        }
    }
}