﻿using Mutators.Enums;
using Mutators.Mutators;
using Mutators.Mutators.Patches;
using Mutators.Settings;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Mutators.Managers
{
    public class MutatorManager
    {
        private static readonly NopMutator _nopMutator = new NopMutator(MutatorSettings.NopMutator);
        public static MutatorManager Instance { get; private set; } = new MutatorManager();

        private readonly IDictionary<string, IMutator> _mutators  = new Dictionary<string, IMutator>();

        public IReadOnlyDictionary<string, IMutator> RegisteredMutators => new ReadOnlyDictionary<string, IMutator>(_mutators);

        public IMutator CurrentMutator { get; internal set; } = _nopMutator;

        public Action<MutatorsGameState> GameStateChanged { get; set; } = default!;

        public MutatorsGameState GameState
        {
            get => _gameState;
            internal set
            {
                if (_gameState == value) return;

                _gameState = value;
                GameStateChanged?.Invoke(value);
            }
        }

        private MutatorsGameState _gameState;

        private IMutator? _previousMutator = null;
        private int _repeatCount = 0;

        private bool _initialized = false;

        internal void InitializeDefaultMutators()
        {
            if (_initialized)
            {
                RepoMutators.Logger.LogWarning("Tried to initialize default mutators, but these were already initialized!");
                return;
            }

            IList<IMutator> mutators = [
                _nopMutator,
                new Mutator(MutatorSettings.OutWithABang, typeof(OutWithABangPatch)),
                new Mutator(MutatorSettings.ApolloEleven, typeof(ApolloElevenPatch)),
                new Mutator(MutatorSettings.UltraViolence, typeof(UltraViolencePatch)),
                new Mutator(MutatorSettings.DuckThis, typeof(DuckThisPatch)),
                new Mutator(MutatorSettings.OneShotOneKill, typeof(OneShotOneKillPatch)),
                new Mutator(MutatorSettings.ProtectThePresident, typeof(ProtectThePresidentPatch), [SemiFunc.IsMultiplayer]),
                new Mutator(MutatorSettings.RustyServos, typeof(RustyServosPatch)),
                new Mutator(MutatorSettings.HandleWithCare, typeof(HandleWithCarePatch)),
                new Mutator(MutatorSettings.HuntingSeason, typeof(HuntingSeasonPatch)),
                new Mutator(MutatorSettings.ThereCanOnlyBeOne, typeof(ThereCanOnlyBeOnePatch)),
                new Mutator(MutatorSettings.VolatileCargo, typeof(VolatileCargoPatch)),
                new Mutator(MutatorSettings.SealedAway, typeof(SealedAwayPatch)),
                new Mutator(MutatorSettings.ProtectTheWeak, typeof(ProtectTheWeakPatch), [SemiFunc.IsMultiplayer]),
                new Mutator(MutatorSettings.FiringMyLaser, typeof(FiringMyLaserPatch), specialActionOverlay: true),
                new Mutator(MutatorSettings.Voiceover, typeof(VoiceoverPatch), [SemiFunc.IsMultiplayer]),
                new Mutator(MutatorSettings.TheFloorIsLava, typeof(TheFloorIsLavaPatch))
                //new Mutator(MutatorSettings.LessIsMore, typeof(LessIsMorePatch)),
                //new Mutator(MutatorSettings.FragmentationProtocol, typeof(FragmentationProtocolPatch))
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

            float lastWeight = _previousMutator?.Settings.Weight ?? 0;
            float totalWeight = eligibleMutators.Sum(item => item.Settings.Weight);

            if (lastWeight > 0 && totalWeight > 0)
            {
                float outlierThreshold = 0.1f;
                float probability = Mathf.Pow(lastWeight / totalWeight, _repeatCount + 1);
                if (probability < outlierThreshold)
                {
                    RepoMutators.Logger.LogDebug($"Cannot pick {_previousMutator?.Name ?? "None"}, threshold reached");
                    eligibleMutators = eligibleMutators.Where(m => m != _previousMutator).ToList();
                    totalWeight = eligibleMutators.Sum(m => m.Settings.Weight);
                }
            }

            if (totalWeight <= 0)
            {
                RepoMutators.Logger.LogWarning($"Fell back to None mutator, invalid total weight: {totalWeight}");
                return _nopMutator;
            }

            float randomValue = UnityEngine.Random.Range(0f, totalWeight);

            float currentSum = 0f;
            foreach (IMutator mutator in eligibleMutators)
            {
                currentSum += mutator.Settings.Weight;
                if (randomValue <= currentSum)
                {
                    if (mutator == _previousMutator)
                    {
                        _repeatCount++;
                    }
                    else
                    {
                        _previousMutator = mutator;
                        _repeatCount = 1;
                    }
                    return mutator;
                }
                    
            }

            RepoMutators.Logger.LogWarning($"Fell back to None mutator, mutator selection failed");
            return _nopMutator;
        }
    }
}
