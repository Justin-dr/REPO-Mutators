using Mutators.Mutators;
using Mutators.Mutators.Patches;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mutators.Managers
{
    public class MutatorManager
    {
        public static MutatorManager Instance { get; private set; } = new MutatorManager();

        private bool _initialized = false;

        private readonly IDictionary<string, IMutator> _mutators  = new Dictionary<string, IMutator>();

        public IReadOnlyDictionary<string, IMutator> RegisteredMutators => new ReadOnlyDictionary<string, IMutator>(_mutators);

        public IMutator CurrentMutator { get; internal set; } = new NopMutator(50);

        internal void InitializeDefaultMutators()
        {
            if (_initialized)
            {
                RepoMutators.Logger.LogWarning("Tried to initialize default mutators, but these were already initialized!");
                return;
            }

            IList<IMutator> mutators = [
                CurrentMutator,
                new Mutator(Mutators.Mutators.OutWithABang, typeof(OutWithABangPatch), 100),
                new Mutator(Mutators.Mutators.ApolloEleven, typeof(ApolloElevenPatch), 50000)
            ];

            mutators.ForEach(mutator => _mutators[mutator.Name] = mutator);
            _initialized = true;
        }

        public void RegisterMutator(Mutator mutator)
        {
            _mutators.Add(mutator.Name, mutator);
        }

        public void UnregisterMutator(Mutator mutator)
        {
            if (!_mutators.Remove(mutator.Name)) return;

            if (mutator.Active)
            {
                mutator.Unpatch();
            }
        }

        public void UnregisterMutator(string name)
        {
            if (!_mutators.TryGetValue(name, out var mutator)) return;

            _mutators.Remove(name);

            if (mutator.Active)
            {
                mutator.Unpatch();
            }
        }

        internal void SetActiveMutator(string name, bool applyPatchNow = true)
        {
            RegisteredMutators.Values
                .Where(mutator => mutator.Active)
                .ForEach(mutator => mutator.Unpatch());

            if (_mutators.TryGetValue(name, out IMutator mutator))
            {
                CurrentMutator = mutator;
                RepoMutators.Logger.LogDebug($"Mutator {name} set as active");
                if (applyPatchNow)
                {
                    CurrentMutator.Patch();
                }
                return;
            }

            RepoMutators.Logger.LogWarning($"Tried to active unknown mutator: {name}.");
            RepoMutators.Logger.LogWarning($"You might be out of sync if you are not the host!");
        }

        internal IMutator GetWeightedMutator()
        {
            ICollection<IMutator> mutators = _mutators.Values;
            float totalWeight = mutators.Sum(item => item.Weight);
            float randomValue = UnityEngine.Random.Range(0f, totalWeight);

            float currentSum = 0f;
            foreach (IMutator mutator in mutators)
            {
                currentSum += mutator.Weight;
                if (randomValue < currentSum)
                    return mutator;
            }

            return mutators.Last();
        }
    }
}
