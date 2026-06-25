using System.Collections.Generic;
using Mutators.Mutators;
using Mutators.Providers.Random;
using Mutators.Providers.Semiwork;
using Mutators.Rules.Registries;
using Mutators.Settings;

namespace Mutators.Services.Selection.Strategies
{
    internal class MoonScalingSelectionStrategy : MutatorSelectionStrategy
    {
        private readonly ModSettings.MoonSettings _moonSettings;
        private readonly ISemiFuncProvider _semiFuncProvider;
        public MoonScalingSelectionStrategy(GeneratedMultiMutatorSelectionRulesRegistry multiRegistry, SingleMutatorSelectionRulesRegistry singleRegistry, ModSettings.MoonSettings moonSettings, ISemiFuncProvider semiFuncProvider, IRepeatSelectionTracker repeatSelectionTracker, IRandomProvider randomProvider, NopMutator fallbackMutator) : base(multiRegistry, singleRegistry, repeatSelectionTracker, randomProvider, fallbackMutator)
        {
            _moonSettings = moonSettings;
            _semiFuncProvider = semiFuncProvider;
        }

        public sealed override IMutator Execute(IList<IMutator> mutators)
        {
            int amountToPick = GetMoonMutatorsAmount();
            
            if (amountToPick == 1)
            {
                return GetSingleMutator(mutators);
            }

            if (!ShouldGenerateMulti(GetMoonGeneratedChance()))
            {
                return TryPickUserDefinedMultiMutatorOrElseGenerate(mutators, amountToPick);
            }

            return PickGeneratedMultiMutator(mutators, amountToPick);
        }
        
        private int GetMoonMutatorsAmount()
        {
            ModSettings.MoonSetting moonSetting = _moonSettings.GetMultiMutatorMoonRange(_semiFuncProvider.MoonLevel());
            return randomProvider.RandomRangeInt(moonSetting.MinimumMutators, moonSetting.MaximumMutators + 1);
        }
        
        private int GetMoonGeneratedChance()
        {
            return _moonSettings.GetMultiMutatorMoonRange(_semiFuncProvider.MoonLevel()).GeneratedChance;
        }
    }
}