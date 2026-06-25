using System;
using System.Collections.Generic;
using Mutators.Mutators;
using Mutators.Providers.Random;
using Mutators.Rules.Registries;
using Mutators.Settings;

namespace Mutators.Services.Selection.Strategies
{
    internal class RandomScalingSelectionStrategy : MutatorSelectionStrategy
    {
        private readonly ModSettings.RandomSettings _randomSettings;
        public RandomScalingSelectionStrategy(GeneratedMultiMutatorSelectionRulesRegistry multiRegistry, SingleMutatorSelectionRulesRegistry singleRegistry, ModSettings.RandomSettings randomSettings, IRepeatSelectionTracker repeatSelectionTracker, IRandomProvider randomProvider, NopMutator fallbackMutator) : base(multiRegistry, singleRegistry, repeatSelectionTracker, randomProvider, fallbackMutator)
        {
            _randomSettings = randomSettings;
        }

        public sealed override IMutator Execute(IList<IMutator> mutators)
        {
            int amountToPick = GetRandomAmountOfMutatorsBetweenByWeight();
            if (amountToPick == 1)
            {
                return GetSingleMutator(mutators);
            }

            if (!ShouldGenerateMulti(GetRandomGeneratedChance(amountToPick)))
            {
                return TryPickUserDefinedMultiMutatorOrElseGenerate(mutators, amountToPick);
            }

            return PickGeneratedMultiMutator(mutators, amountToPick);
        }
        
        private int GetRandomGeneratedChance(int amountOfMutators)
        {
            return _randomSettings.GetGeneratedChance(amountOfMutators);
        }
        
        private int GetRandomAmountOfMutatorsBetweenByWeight()
        {
            int minimumAmount = _randomSettings.MinimumAmount;
            int maximumAmount = _randomSettings.MaximumAmount;
            
            if (minimumAmount < 1 || minimumAmount > ModSettings.MaximumGeneratedActiveSubMutators)
            {
                throw new ArgumentException(
                    $"Minimum amount of Mutators should be between 1 and {ModSettings.MaximumGeneratedActiveSubMutators}",
                    nameof(minimumAmount)
                );
            }

            if (maximumAmount < 1 || maximumAmount > ModSettings.MaximumGeneratedActiveSubMutators)
            {
                throw new ArgumentException(
                    $"Maximum amount of Mutators should be between 1 and {ModSettings.MaximumGeneratedActiveSubMutators}",
                    nameof(maximumAmount)
                );
            }

            if (minimumAmount > maximumAmount)
            {
                throw new ArgumentException(
                    "Minimum amount of Mutators should not be greater than maximum amount of Mutators.",
                    nameof(minimumAmount)
                );
            }

            int totalWeight = 0;
            for (int amount = minimumAmount; amount <= maximumAmount; amount++)
            {
                totalWeight += _randomSettings.GetWeight(amount);
            }

            if (totalWeight <= 0)
            {
                return randomProvider.RandomRangeInt(minimumAmount, maximumAmount + 1);
            }

            int randomValue = randomProvider.RandomRangeInt(0, totalWeight);
            int currentWeight = 0;

            for (int amount = minimumAmount; amount <= maximumAmount; amount++)
            {
                currentWeight += _randomSettings.GetWeight(amount);
                if (randomValue < currentWeight)
                {
                    return amount;
                }
            }

            return maximumAmount;
        }
    }
}