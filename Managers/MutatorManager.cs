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
        private static readonly NopMutator _nopMutator = new NopMutator(Settings.NopMutatorWeight.Value);
        public static MutatorManager Instance { get; private set; } = new MutatorManager();

        private readonly IDictionary<string, IMutator> _mutators  = new Dictionary<string, IMutator>();

        internal IDictionary<string, string> metadata = new Dictionary<string, string>();

        public IReadOnlyDictionary<string, IMutator> RegisteredMutators => new ReadOnlyDictionary<string, IMutator>(_mutators);

        public IMutator CurrentMutator { get; internal set; } = _nopMutator;

        public IReadOnlyDictionary<string, string> Metadata => new ReadOnlyDictionary<string, string>(metadata);

        private bool _initialized = false;

        public Action<IDictionary<string, string>> OnMetadataChanged { get; set; } = null!;

        internal void InitializeDefaultMutators()
        {
            if (_initialized)
            {
                RepoMutators.Logger.LogWarning("Tried to initialize default mutators, but these were already initialized!");
                return;
            }

            IList<IMutator> mutators = [
                _nopMutator,
                new Mutator(Mutators.Mutators.OutWithABang, typeof(OutWithABangPatch), Settings.OutWithABangWeight.Value),
                new Mutator(Mutators.Mutators.ApolloEleven, typeof(ApolloElevenPatch), Settings.AppoloElevenWeight.Value),
                new Mutator(Mutators.Mutators.UltraViolence, typeof(UltraViolencePatch), Settings.UltraViolenceWeight.Value),
                new Mutator(Mutators.Mutators.DuckThis, typeof(DuckThisPatch), Settings.DuckThisWeight.Value),
                new Mutator(Mutators.Mutators.OneShotOneKill, typeof(OneShotOneKillPatch), Settings.OneShotOneKillWeight.Value),
                new Mutator(Mutators.Mutators.ProtectThePresident, typeof(ProtectThePresidentPatch), Settings.ProtectThePresidentWeight.Value, [SemiFunc.IsMultiplayer, ProtectThePresidentPatch.CanBePicked]),
                new Mutator(Mutators.Mutators.RustyServos, typeof(RustyServosPatch), Settings.RustyServosWeight.Value),
                new Mutator(Mutators.Mutators.HandleWithCare, typeof(HandleWithCarePatch), Settings.HandleWithCareWeight.Value),
                new Mutator(Mutators.Mutators.HuntingSeason, typeof(HuntingSeasonPatch), Settings.HuntingSeasonWeight.Value),
            ];

            mutators.ForEach(mutator => _mutators[mutator.Name] = mutator);
            _initialized = true;
        }

        public void RegisterMutator(IMutator mutator)
        {
            _mutators.Add(mutator.Name, mutator);
        }

        public void UnregisterMutator(IMutator mutator)
        {
            if (!_mutators.Remove(mutator.Name)) return;

            if (mutator.Active)
            {
                mutator.Unpatch();
            }
        }

        public void UnregisterMutator(string name)
        {
            if (!_mutators.TryGetValue(name, out IMutator mutator)) return;

            UnregisterMutator(mutator);
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
            float totalWeight = mutators.Where(mutator => mutator.Conditions.All(condition => condition())).Sum(item => item.Weight);

            if (totalWeight <= 0)
            {
                return _nopMutator;
            }

            float randomValue = UnityEngine.Random.Range(0f, totalWeight);

            float currentSum = 0f;
            foreach (IMutator mutator in mutators)
            {
                currentSum += mutator.Weight;
                if (randomValue < currentSum)
                    return mutator;
            }

            return _nopMutator;
        }
    }
}
