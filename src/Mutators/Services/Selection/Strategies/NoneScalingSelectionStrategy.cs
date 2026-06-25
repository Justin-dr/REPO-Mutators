using System.Collections.Generic;
using Mutators.Extensions;
using Mutators.Mutators;
using Mutators.Providers.Random;
using Mutators.Rules.Registries;

namespace Mutators.Services.Selection.Strategies
{
    internal class NoneScalingSelectionStrategy : MutatorSelectionStrategy
    {
        public NoneScalingSelectionStrategy(GeneratedMultiMutatorSelectionRulesRegistry multiRegistry, SingleMutatorSelectionRulesRegistry singleRegistry, IRepeatSelectionTracker repeatSelectionTracker, IRandomProvider randomProvider, NopMutator fallbackMutator) : base(multiRegistry, singleRegistry, repeatSelectionTracker, randomProvider, fallbackMutator)
        {
        }

        public sealed override IMutator Execute(IList<IMutator> mutators)
        {
            return PickSingleMutator(
                mutators.GetSelection(mutator => mutator is not IMultiMutator or IMultiMutator { SubMutators.Count: 1 }),
                new MutatorSelectionContext(1)
            );
        }
    }
}