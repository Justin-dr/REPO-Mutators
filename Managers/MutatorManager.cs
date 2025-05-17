using HarmonyLib;
using Mutators.Mutators;
using Mutators.Mutators.Patches;
using Mutators.Settings;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mutators.Managers
{
    public class MutatorManager
    {
        private static readonly NopMutator _nopMutator = new NopMutator(MutatorSettings.NopMutator);
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
                new Mutator(Mutators.Mutators.OutWithABang, typeof(OutWithABangPatch), MutatorSettings.OutWithABang),
                new Mutator(Mutators.Mutators.ApolloEleven, typeof(ApolloElevenPatch), MutatorSettings.ApolloEleven),
                new Mutator(Mutators.Mutators.UltraViolence, typeof(UltraViolencePatch), MutatorSettings.UltraViolence),
                new Mutator(Mutators.Mutators.DuckThis, typeof(DuckThisPatch), MutatorSettings.DuckThis),
                new Mutator(Mutators.Mutators.OneShotOneKill, typeof(OneShotOneKillPatch), MutatorSettings.OneShotOneKill),
                new Mutator(Mutators.Mutators.ProtectThePresident, typeof(ProtectThePresidentPatch), MutatorSettings.ProtectThePresident, [SemiFunc.IsMultiplayer]),
                new Mutator(Mutators.Mutators.RustyServos, typeof(RustyServosPatch), MutatorSettings.RustyServos),
                new Mutator(Mutators.Mutators.HandleWithCare, typeof(HandleWithCarePatch), MutatorSettings.HandleWithCare),
                new Mutator(Mutators.Mutators.HuntingSeason, typeof(HuntingSeasonPatch), MutatorSettings.HuntingSeason),
                new Mutator(Mutators.Mutators.ThereCanOnlyBeOne, typeof(ThereCanOnlyBeOnePatch), MutatorSettings.ThereCanOnlyBeOne),
                new Mutator(Mutators.Mutators.VolatileCargo, typeof(VolatileCargoPatch), MutatorSettings.VolatileCargo)
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
            IList<IMutator> eligibleMutators = _mutators.Values
                .Where(mutator => mutator.Settings.IsEligibleForSelection())
                .Where(mutator => mutator.Conditions.All(condition => condition()))
                .ToList();

            float totalWeight = eligibleMutators.Sum(item => item.Settings.Weight);

            if (totalWeight <= 0)
            {
                return _nopMutator;
            }

            float randomValue = UnityEngine.Random.Range(0f, totalWeight);

            float currentSum = 0f;
            foreach (IMutator mutator in eligibleMutators)
            {
                currentSum += mutator.Settings.Weight;
                if (randomValue < currentSum)
                    return mutator;
            }

            return _nopMutator;
        }
    }
}
