using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mutators.Announcements;
using Mutators.Assets;
using Mutators.Enums;
using Mutators.Extensions;
using Mutators.Mutators;
using Mutators.Mutators.MultiMutators;
using Mutators.Mutators.Patches;
using Mutators.Providers.Semiwork;
using Mutators.Rules.Registries;
using Mutators.Services.Selection;
using Mutators.Settings;

namespace Mutators.Managers
{
    /// <summary>
    /// Central manager for all Mutators. Responsible for registering and unregistering mutators,
    /// and for managing the current mutator.
    /// <para>
    /// Delegates to <see cref="IMutatorSelectionService"/> for
    /// selecting the current mutator.
    /// </para>
    /// </summary>
    public class MutatorManager
    {
        /// <summary>
        /// SemiFunc wrapper to keep the actual SemiFunc dependency out of the mod's core.
        /// </summary>
        private ISemiFuncProvider SemiFunc { get; }

        /// <summary>
        /// The singleton instance of the <see cref="MutatorManager"/>.
        /// </summary>
        public static MutatorManager Instance { get; private set; } = DI.Container.Resolve<MutatorManager>();

        private readonly IDictionary<string, IMutator> _mutators  = new Dictionary<string, IMutator>();
        
        /// <summary>
        /// Registry for generated multi-mutator selection rules.
        /// </summary>
        public GeneratedMultiMutatorSelectionRulesRegistry GeneratedMultiMutatorSelectionRulesRegistry { get; }
        
        /// <summary>
        /// Registry for single-mutator selection rules.
        /// </summary>
        public SingleMutatorSelectionRulesRegistry SingleMutatorSelectionRulesRegistry { get; }

        /// <summary>
        /// Readonly dictionary of all registered mutators, keyed by <see cref="IMutator.NamespacedName"/>.
        /// </summary>
        public IReadOnlyDictionary<string, IMutator> RegisteredMutators => new ReadOnlyDictionary<string, IMutator>(_mutators);

        /// <summary>
        /// The current Mutator. This may be a single mutator or a multi-mutator.
        /// <remarks>
        /// The current Mutator is the one that is currently scheduled. It may not currently be active.
        /// <para>
        /// Use <see cref="IMutator.Active"/> to check if the current Mutator is active.
        /// </para>
        /// </remarks>
        /// </summary>
        public IMutator CurrentMutator { get; internal set; }

        /// <summary>
        /// Fired when the Mutators game state changes.
        /// </summary>
        public event Action<MutatorsGameState> GameStateChanged;

        /// <summary>
        /// The current Mutators game state.
        /// </summary>
        public MutatorsGameState GameState
        {
            get => _gameState;
            internal set
            {
                if (_gameState == value) return;

                _gameState = value;
                GameStateChanged.Invoke(value);
            }
        }
        
        private readonly IMutatorSelectionService _mutatorSelectionService;

        private MutatorsGameState _gameState;
        
        private bool _initialized;

        internal MutatorManager(IMutatorSelectionService mutatorSelectionService, ISemiFuncProvider semiFunc, GeneratedMultiMutatorSelectionRulesRegistry generatedMultiMutatorSelectionRulesRegistry, SingleMutatorSelectionRulesRegistry singleMutatorSelectionRulesRegistry, NopMutator currentMutator)
        {
            _mutatorSelectionService = mutatorSelectionService;
            SemiFunc = semiFunc;
            GeneratedMultiMutatorSelectionRulesRegistry = generatedMultiMutatorSelectionRulesRegistry;
            SingleMutatorSelectionRulesRegistry = singleMutatorSelectionRulesRegistry;
            CurrentMutator = currentMutator;
        }

        internal void InitializeDefaultMutators()
        {
            if (_initialized)
            {
                RepoMutators.Logger.LogWarning("Tried to initialize default mutators, but these were already initialized!");
                return;
            }

            IList<IMutator> mutators = [
                DI.Container.Resolve<NopMutator>(),
                new Mutator(MutatorSettings.OutWithABang, typeof(OutWithABangPatch), MutatorDifficulty.Normal, [() => AssetStore.Preset]),
                new Mutator(MutatorSettings.ApolloEleven, typeof(ApolloElevenPatch), MutatorDifficulty.Normal),
                new Mutator(MutatorSettings.UltraViolence, typeof(UltraViolencePatch), MutatorDifficulty.Hard),
                new Mutator(MutatorSettings.DuckThis, typeof(DuckThisPatch), MutatorDifficulty.Normal),
                new Mutator(MutatorSettings.OneShotOneKill, typeof(OneShotOneKillPatch), MutatorDifficulty.Hard),
                new Mutator(MutatorSettings.ProtectThePresident, typeof(ProtectThePresidentPatch), MutatorDifficulty.Normal, [SemiFunc.IsMultiplayer]),
                new Mutator(MutatorSettings.RustyServos, typeof(RustyServosPatch), MutatorDifficulty.Normal),
                new Mutator(MutatorSettings.HandleWithCare, typeof(HandleWithCarePatch), MutatorDifficulty.Normal),
                new Mutator(MutatorSettings.HuntingSeason, typeof(HuntingSeasonPatch), MutatorDifficulty.Hard),
                new Mutator(MutatorSettings.ThereCanOnlyBeOne, typeof(ThereCanOnlyBeOnePatch), MutatorDifficulty.Normal),
                new Mutator(MutatorSettings.VolatileCargo, typeof(VolatileCargoPatch), MutatorDifficulty.Normal, [() => AssetStore.Preset]),
                new Mutator(MutatorSettings.SealedAway, typeof(SealedAwayPatch), MutatorDifficulty.Normal),
                new Mutator(MutatorSettings.ProtectTheWeak, typeof(ProtectTheWeakPatch), MutatorDifficulty.Normal, [SemiFunc.IsMultiplayer]),
                new Mutator(MutatorSettings.FiringMyLaser, typeof(FiringMyLaserPatch), MutatorDifficulty.Normal, [() => AssetStore.IsLaserLoaded], specialActionOverlay: true),
                new Mutator(MutatorSettings.Voiceover, typeof(VoiceoverPatch), MutatorDifficulty.Normal, [SemiFunc.IsMultiplayer]),
                new Mutator(MutatorSettings.TheFloorIsLava, typeof(TheFloorIsLavaPatch), MutatorDifficulty.Hard),
                new Mutator(MutatorSettings.LessIsMore, typeof(LessIsMorePatch), MutatorDifficulty.Normal),
                new Mutator(MutatorSettings.Amalgam, typeof(AmalgamPatch), MutatorDifficulty.Normal),
                new Mutator(MutatorSettings.NullSignal, typeof(NullSignalPatch), MutatorDifficulty.Normal),
                new Mutator(MutatorSettings.SizeMatters, typeof(SizeMattersPatch), MutatorDifficulty.Normal)
            ];

            mutators.ForEach(RegisterMutator);

            GameStateChanged += gameState =>
            {
                if (gameState == MutatorsGameState.LevelReady)
                {
                    LevelManager.Instance.RestoreLevels();
                }
            };

            _initialized = true;
        }

        internal void InitializeMultiMutators()
        {
            IList<IMultiMutator> mutators = MultiMutatorLoader.LoadAll();
            mutators.ForEach(RegisterMutator);
            RepoMutators.Logger.LogInfo($"Loaded {mutators.Count} MultiMutator{(mutators.Count == 1 ? "" : "s")}!");
        }

        /// Registers a mutator.
        /// <param name="mutator">
        /// The mutator to register, identified by its `NamespacedName` property.
        /// The `NamespacedName` property must be unique.
        /// </param>
        public void RegisterMutator(IMutator mutator)
        {
            RepoMutators.Logger.LogDebug("Registering mutator " + mutator.NamespacedName);
            if (_mutators.TryAdd(mutator.NamespacedName, mutator))
            {
                RepoMutators.Logger.LogInfo($"Successfully registered mutator {mutator.NamespacedName}!");
            }
        }

        /// Unregisters a mutator.
        /// <param name="mutator">
        /// The mutator to unregister, identified by its NamespacedName property.
        /// If the mutator is active, its patches are unpatched during its removal.
        /// </param>
        public void UnregisterMutator(IMutator mutator)
        {
            if (!_mutators.Remove(mutator.NamespacedName)) return;

            if (mutator.Active)
            {
                mutator.Unpatch();
            }
            RepoMutators.Logger.LogInfo("Successfully unregistered mutator " + mutator.NamespacedName);
        }

        /// <summary>
        /// Unregisters a mutator by its NamespacedName.
        /// If the mutator is active, its patches are unpatched during its removal.
        /// </summary>
        /// <param name="namespacedName">The unique identifier of the mutator.</param>
        public void UnregisterMutator(string namespacedName)
        {
            if (!_mutators.TryGetValue(namespacedName, out IMutator mutator)) return;

            UnregisterMutator(mutator);
        }

        /// <summary>
        /// Check if a mutator with the supplied mutator's NamespacedName is the current mutator.
        /// Or in case of a multi-mutator, if any of its sub-mutators match the NamespacedName.
        /// </summary>
        /// <param name="mutator">The mutator whose NamespacedName is used for the search.</param>
        public bool HasCurrentMutator(IMutator mutator)
        {
            return HasCurrentMutator(mutator.NamespacedName);
        }

        /// <summary>
        /// Check if a mutator with the supplied NamespacedName is the current mutator.
        /// Or in case of a multi-mutator, if any of its sub-mutators match the NamespacedName.
        /// </summary>
        /// <param name="namespacedName">The unique identifier of the mutator.</param>
        public bool HasCurrentMutator(string namespacedName)
        {
            if (CurrentMutator.NamespacedName == namespacedName)
            {
                return true;
            }

            if (CurrentMutator is not IMultiMutator multiMutator)
            {
                return false;
            }

            // Using loop to avoid unnecessary LINQ allocations
            // Since I can see this running in a hot-path
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (IMutator subMutator in multiMutator.SubMutators.Keys)
            {
                if (subMutator.NamespacedName == namespacedName)
                {
                    return true;
                }
            }

            return false;
        }

        internal void SetActiveMutator(string namespacedName, bool applyPatchNow = true)
        {
            if (!_mutators.TryGetValue(namespacedName, out IMutator mutator))
            {
                RepoMutators.Logger.LogWarning($"Tried to activate unknown mutator: {namespacedName}.");
                RepoMutators.Logger.LogWarning("You might be out of sync if you are not the host!");
                return;
            }

            SetActiveMutator(mutator, applyPatchNow);
        }

        internal void SetActiveMutator(IMutator mutator, bool applyPatchNow = true)
        {
            CurrentMutator.Unpatch();
            RegisteredMutators.Values
                .Where(registeredMutator => registeredMutator.Active)
                .ForEach(activeMutator => activeMutator.Unpatch());

            CurrentMutator = mutator;
            RepoMutators.Logger.LogDebug($"Mutator {mutator.NamespacedName} set as active");
            if (applyPatchNow)
            {
                CurrentMutator.Patch();
            }
            MutatorAnnouncingBag.Instance.Prime(CurrentMutator);
        }

        internal IMutator GetWeightedMutator()
        {
            return _mutatorSelectionService.GetWeightedMutator(_mutators.Values);
        }
    }
}
