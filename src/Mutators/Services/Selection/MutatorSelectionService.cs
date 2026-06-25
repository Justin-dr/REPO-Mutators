using System.Collections.Generic;
using System.Linq;
using Mutators.Mutators;
using Mutators.Providers.Random;
using Mutators.Services.Selection.Strategies;
using Mutators.Settings;

namespace Mutators.Services.Selection
{
    /// <summary>
    /// A service used to select a weighted mutator from a collection of mutators.
    /// <para>
    /// Delegates to <see cref="MutatorSelectionStrategy">MutatorSelectionStrategies</see> based on the configured <see cref="ModSettings.MultiMutatorScalingType"/>.
    /// </para>
    /// </summary>
    public class MutatorSelectionService : IMutatorSelectionService
    {
        private readonly IDictionary<ModSettings.MultiMutatorScalingType, MutatorSelectionStrategy> _selectionStrategies;
        private readonly NopMutator _nopMutator;
        private readonly IRepeatSelectionTracker _repeatSelectionTracker;
        private readonly IRandomProvider _randomProvider;
        
        internal MutatorSelectionService(
            NoneScalingSelectionStrategy noneScalingSelectionStrategy,
            MoonScalingSelectionStrategy moonScalingSelectionStrategy,
            RandomScalingSelectionStrategy randomScalingSelectionStrategy,
            IRepeatSelectionTracker repeatSelectionTracker,
            IRandomProvider randomProvider,
            NopMutator nopMutator
        )
        {
            _selectionStrategies = new Dictionary<ModSettings.MultiMutatorScalingType, MutatorSelectionStrategy>()
            {
                { ModSettings.MultiMutatorScalingType.None, noneScalingSelectionStrategy },
                { ModSettings.MultiMutatorScalingType.Moon, moonScalingSelectionStrategy },
                { ModSettings.MultiMutatorScalingType.Random, randomScalingSelectionStrategy }
            };
            _repeatSelectionTracker = repeatSelectionTracker;
            _randomProvider = randomProvider;
            _nopMutator = nopMutator;
        }

        /// <summary>
        /// <inheritdoc cref="IMutatorSelectionService.GetWeightedMutator"/>
        /// </summary>
        /// <param name="mutators"><inheritdoc cref="IMutatorSelectionService.GetWeightedMutator"/></param>
        /// <returns><inheritdoc cref="IMutatorSelectionService.GetWeightedMutator"/></returns>
        public IMutator GetWeightedMutator(ICollection<IMutator> mutators)
        {
            ModSettings.MultiMutatorScalingType scalingType = RepoMutators.Settings.MutatorScalingType;
            
            if (ShouldUseNopMutator(_nopMutator))
            {
                return _repeatSelectionTracker.TrackSelectedMutator(_nopMutator);
            }

            IList<IMutator> weightedMutators = mutators
                .Where(mutator => mutator is not NopMutator && mutator.Settings.Weight > 0)
                .ToList();

            return _selectionStrategies[scalingType].Execute(weightedMutators);
        }
        
        private bool ShouldUseNopMutator(NopMutator nopMutator)
        {
            int chance = nopMutator.Settings.Weight;
            if (chance <= 0)
            {
                return false;
            }

            float probability = chance / 100f;
            if (_repeatSelectionTracker.ShouldBlockRepeat(nopMutator, probability))
            {
                RepoMutators.Logger.LogDebug($"Cannot pick {nopMutator.Name}, threshold reached");
                return false;
            }

            return chance >= 100 || _randomProvider.Range(0f, 100f) <= chance;
        }
    }
}