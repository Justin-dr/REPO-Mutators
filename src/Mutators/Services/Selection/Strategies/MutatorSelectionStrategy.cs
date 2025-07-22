using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mutators.Extensions;
using Mutators.Mutators;
using Mutators.Providers.Random;
using Mutators.Rules.Registries;
using Mutators.Settings;

namespace Mutators.Services.Selection.Strategies
{
    internal abstract class MutatorSelectionStrategy
    {
        private readonly IMutator _fallbackMutator;
        private readonly GeneratedMultiMutatorSelectionRulesRegistry _multiRegistry;
        private readonly SingleMutatorSelectionRulesRegistry _singleRegistry;
        protected readonly IRandomProvider randomProvider;
        private readonly IRepeatSelectionTracker _repeatSelectionTracker;
        internal MutatorSelectionStrategy(GeneratedMultiMutatorSelectionRulesRegistry multiRegistry, SingleMutatorSelectionRulesRegistry singleRegistry, IRepeatSelectionTracker repeatSelectionTracker, IRandomProvider randomProvider, NopMutator nopMutator)
        {
            _multiRegistry = multiRegistry;
            _singleRegistry = singleRegistry;
            _repeatSelectionTracker = repeatSelectionTracker;
            this.randomProvider = randomProvider;
            _fallbackMutator = nopMutator;
        }

        public abstract IMutator Execute(IList<IMutator> mutators);
        
        protected IMutator PickSingleMutator(IList<IMutator> mutators, MutatorSelectionContext context)
        {
            if (context.AmountToPick == 1)
            {
                mutators = mutators.Where(m => _singleRegistry.RunRules(m)).ToList();
            }

            IMutator? previousMutator = _repeatSelectionTracker.PreviousMutator;
            float totalWeight = mutators.Sum(item => item.Settings.Weight);

            if (previousMutator != null && mutators.Contains(previousMutator) && totalWeight > 0)
            {
                float lastWeight = previousMutator.Settings.Weight;
                if (lastWeight > 0 && _repeatSelectionTracker.ShouldBlockRepeat(previousMutator, lastWeight / totalWeight))
                {
                    RepoMutators.Logger.LogDebug($"Cannot pick {previousMutator?.Name ?? "None"}, threshold reached");
                    mutators = mutators.Where(m => m != previousMutator).ToList();
                    totalWeight = mutators.Sum(m => m.Settings.Weight);
                }
            }

            if (totalWeight <= 0)
            {
                RepoMutators.Logger.LogWarning($"Fell back to None mutator, invalid total weight: {totalWeight}");
                return _fallbackMutator;
            }

            float randomValue = randomProvider.Range(0f, totalWeight);

            float currentSum = 0f;
            foreach (IMutator mutator in mutators)
            {
                currentSum += mutator.Settings.Weight;

                if (randomValue <= currentSum)
                {
                    return _repeatSelectionTracker.TrackSelectedMutator(mutator);
                }
                    
            }

            RepoMutators.Logger.LogWarning("Fell back to None mutator, mutator selection failed");
            return _fallbackMutator;
        }
        
        protected IMutator PickGeneratedMultiMutator(IList<IMutator> mutators, int amountToPick)
        {
            IList<IMutator> excludeList = [_fallbackMutator];
            IList<IMutator> pickedMutators = [];
            
            MutatorSelectionContext context = new MutatorSelectionContext((uint) amountToPick);
            IList<IMutator> eligibleMutators = mutators.GetSelection(mutator => mutator is not IMultiMutator && !excludeList.Contains(mutator));
            for (int i = 0; i < amountToPick; i++)
            {
                IList<IMutator> generatedMutatorSelectionResult = eligibleMutators.GetSelection(mutator => ApplyGenerationRules(pickedMutators, mutator));
                
                IMutator mutator = PickSingleMutator(generatedMutatorSelectionResult, context);

                if (mutator == _fallbackMutator)
                {
                    continue;
                }
                
                pickedMutators.Add(mutator);
            }

            if (pickedMutators.Count == 0)
            {
                return _fallbackMutator;
            }

            if (pickedMutators.Count == 1)
            {
                return pickedMutators.First();
            }

            return new MultiMutator(
                new MultiMutatorSettings(MyPluginInfo.PLUGIN_GUID, string.Empty, string.Empty),
                pickedMutators.ToDictionary(m => m, IDictionary<string, object> (_) => new Dictionary<string, object>())
            );
        }
        
        private bool ApplyGenerationRules(IList<IMutator> pickedMutators, IMutator mutator)
        {
            if (pickedMutators.Contains(mutator))
            {
                return false;
            }

            return _multiRegistry.RunRules(new ReadOnlyCollection<IMutator>(pickedMutators), mutator);
        }
        
        protected bool ShouldGenerateMulti(int generatedChance)
        {
            if (generatedChance <= 0)
            {
                if (generatedChance < 0)
                {
                    LogMisconfiguredMultiMutatorChance(generatedChance);
                    RepoMutators.Logger.LogWarning("The minimum allowed value is 0.");
                }
                return false;
            }

            if (generatedChance > 100)
            {
                LogMisconfiguredMultiMutatorChance(generatedChance);
                RepoMutators.Logger.LogWarning("The maximum allowed value is 100.");
                return true;
            }

            return generatedChance == 100 || randomProvider.RandomRangeInt(1, 101) <= generatedChance;
        }

        protected IMutator GetSingleMutator(IList<IMutator> mutators)
        {
            return PickSingleMutator(
                mutators.GetSelection(mutator => mutator is not IMultiMutator or IMultiMutator { SubMutators.Count: 1 }).ToList(),
                new MutatorSelectionContext(1)
            );
        }

        protected IMutator TryPickUserDefinedMultiMutatorOrElseGenerate(IList<IMutator> mutators, int amountOfSubMutators)
        {
            IList<IMutator> userDefinedMultiMutators = mutators.GetSelection(mutator => mutator is IMultiMutator multi
                && multi.SubMutators.Count == amountOfSubMutators).ToList();

            if (userDefinedMultiMutators.Select(m => m.Settings.Weight).Sum() > 0)
            {
                return PickSingleMutator(
                    userDefinedMultiMutators,
                    new MutatorSelectionContext(1)
                );
            }

            RepoMutators.Logger.LogDebug($"No user defined multi-mutator found for {amountOfSubMutators} sub-mutators.");
            return PickGeneratedMultiMutator(mutators, amountOfSubMutators);
        }

        private static void LogMisconfiguredMultiMutatorChance(int generatedChance)
        {
            RepoMutators.Logger.LogWarning($"Generated multi-mutator chance is misconfigured: {generatedChance}.");
        }
    }
}