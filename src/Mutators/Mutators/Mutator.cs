using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Mutators.Enums;
using Mutators.Extensions;
using Mutators.Managers;
using Mutators.Settings;

namespace Mutators.Mutators
{
    /// <summary>
    /// Default <see cref="IMutator"/> implementation for a single mutator backed by one or more Harmony patch types.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Activating it applies every configured patch type using a Harmony instance identified by
    /// <see cref="NamespacedName"/>; deactivating it removes those patches and clears metadata and runtime overrides.
    /// </para>
    /// <para>
    /// Patch types may declare the static, parameterless lifecycle hooks <c>BeforePatchAll</c>,
    /// <c>AfterPatchAll</c>, <c>BeforeUnpatchAll</c>, and <c>AfterUnpatchAll</c>. They may also declare
    /// <c>OnMetadataChanged(IDictionary&lt;string, object&gt;)</c> to receive merged metadata updates.
    /// </para>
    /// <para>
    /// Metadata required before a level is ready is deferred until <see cref="MutatorsGameState.LevelReady"/>,
    /// except for keys exposed by the mutator settings, which are applied immediately.
    /// </para>
    /// </remarks>
    public class Mutator : IMutator
    {
        private readonly Harmony _harmony;
        private readonly IList<Type> _patches;
        private readonly IList<Func<bool>> _conditions;
        private readonly IDictionary<string, object> _pendingDeferredMetadata = new Dictionary<string, object>();
        private IDictionary<string, object> _metadata;

        // HOOKS
        private readonly IList<Action> _beforePatchAllHooks = [];
        private readonly IList<Action> _afterPatchAllHooks = [];
        private readonly IList<Action<IDictionary<string, object>>> _onMetadataChangedHooks = [];
        private readonly IList<Action> _beforeUnpatchAllHooks = [];
        private readonly IList<Action> _afterUnpatchAllHooks = [];

        // PROPS
        /// <summary>
        /// <inheritdoc cref="IMutator.NamespacedName"/>
        /// </summary>
        public string NamespacedName => Settings.NamespacedName;
        
        /// <summary>
        /// <inheritdoc cref="IMutator.Name"/>
        /// </summary>
        public string Name => Settings.MutatorName;
        
        /// <summary>
        /// <inheritdoc cref="IMutator.Description"/>
        /// </summary>
        public string Description => Settings.MutatorDescription;
        
        /// <summary>
        /// <inheritdoc cref="IMutator.Difficulty"/>
        /// </summary>
        public MutatorDifficulty Difficulty { get; }
        
        /// <summary>
        /// <inheritdoc cref="IMutator.Source"/>
        /// </summary>
        public MutatorSource Source => MutatorSource.Mod;
        
        /// <summary>
        /// <inheritdoc cref="IMutator.Active"/>
        /// </summary>
        public bool Active { get; private set; }
        
        /// <summary>
        /// <inheritdoc cref="IMutator.HasSpecialAction"/>
        /// </summary>
        public bool HasSpecialAction { get; }
        
        /// <summary>
        /// <inheritdoc cref="IMutator.Settings"/>
        /// </summary>
        public AbstractMutatorSettings Settings { get; }
        
        /// <summary>
        /// <inheritdoc cref="IMutator.Conditions"/>
        /// </summary>
        public IReadOnlyList<Func<bool>> Conditions => new ReadOnlyCollection<Func<bool>>(_conditions);
        
        /// <summary>
        /// <inheritdoc cref="IMutator.Patches"/>
        /// </summary>
        public IReadOnlyList<Type> Patches => new ReadOnlyCollection<Type>(_patches);

        /// <summary>
        /// Initializes a mutator backed by one or more Harmony patch types.
        /// </summary>
        /// <param name="settings">The settings that define the mutator's identity, description, weight, and level eligibility.</param>
        /// <param name="patches">The Harmony patch types applied while the mutator is active.</param>
        /// <param name="difficulty">The difficulty of the mutator.</param>
        /// <param name="conditions">Additional conditions that must all pass before the mutator is eligible for selection.</param>
        /// <param name="specialActionOverlay">Whether the mutator uses the built-in special-action overlay.</param>
        public Mutator(AbstractMutatorSettings settings, IList<Type> patches, MutatorDifficulty difficulty, IList<Func<bool>>? conditions = null, bool specialActionOverlay = false)
        {
            Settings = settings;
            _harmony = new Harmony(NamespacedName);
            _patches = patches ?? [];
            Difficulty = difficulty;
            _conditions = conditions ?? [];
            _metadata = new Dictionary<string, object>();
            HasSpecialAction = specialActionOverlay;

            foreach (Type patch in _patches)
            {
                TryAddHook(patch, "BeforePatchAll", _beforePatchAllHooks);
                TryAddHook(patch, "AfterPatchAll", _afterPatchAllHooks);
                TryAddHook(patch, "BeforeUnpatchAll", _beforeUnpatchAllHooks);
                TryAddHook(patch, "AfterUnpatchAll", _afterUnpatchAllHooks);

                TryAddMetadataHook(patch);
            }
        }

        /// <summary>
        /// Initializes a mutator backed by a single Harmony patch type.
        /// </summary>
        /// <param name="settings">The settings that define the mutator's identity, description, weight, and level eligibility.</param>
        /// <param name="patch">The Harmony patch type applied while the mutator is active.</param>
        /// <param name="difficulty">The difficulty of the mutator.</param>
        /// <param name="conditions">Additional conditions that must all pass before the mutator is eligible for selection.</param>
        /// <param name="specialActionOverlay">Whether the mutator uses the built-in special-action overlay.</param>
        public Mutator(AbstractMutatorSettings settings, Type patch, MutatorDifficulty difficulty, IList<Func<bool>> conditions = null!, bool specialActionOverlay = false) : this(settings, [patch], difficulty, conditions, specialActionOverlay)
        {
            
        }
        
        /// <summary>
        /// <inheritdoc cref="IMutator.Patch"/>
        /// <para>
        /// Actives the mutator's Harmony patches. Calls the patches' <c>BeforePatchAll</c> and <c>AfterPatchAll</c> hooks
        /// before and after this mutator's patches are applied, respectively.
        /// </para>
        /// </summary>
        public void Patch()
        {
            RepoMutators.Logger.LogDebug($"{NamespacedName} active: {Active}");
            if (Active) return;

            RepoMutators.Logger.LogDebug($"About to apply {_patches.Count} patches for {NamespacedName}");

            Active = true;

            MutatorManager.Instance.GameStateChanged += TryApplyDeferredMetadata;

            _beforePatchAllHooks.ForEach(action => action?.Invoke());

            foreach (Type patch in _patches)
            {
                _harmony.PatchAll(patch);
                RepoMutators.Logger.LogDebug($"Applied patch: {patch.Name}");
            }

            _afterPatchAllHooks.ForEach(action => action?.Invoke());
        }

        /// <summary>
        /// <inheritdoc cref="IMutator.Unpatch"/>
        /// <para>
        /// Deactivates the mutator's Harmony patches. Calls the patches' <c>BeforeUnpatchAll</c> and <c>AfterUnpatchAll</c> hooks
        /// before and after this mutator's patches are unapplied, respectively.
        /// </para>
        /// <remarks>
        /// Also clears any metadata and runtime overrides that this mutator may have been holding on to.
        /// </remarks>
        /// </summary>
        public void Unpatch()
        {
            if (!Active) return;

            MutatorManager.Instance.GameStateChanged -= TryApplyDeferredMetadata;

            _metadata.Clear();
            _pendingDeferredMetadata.Clear();

            _beforeUnpatchAllHooks.ForEach(action => action?.Invoke());

            _harmony.UnpatchSelf();
            Active = false;

            _afterUnpatchAllHooks.ForEach(action => action?.Invoke());
            Settings.ClearRuntimeOverrides(); // Also cleared in Multi but better safe than sorry in this case.
            RepoMutators.Logger.LogDebug($"Unpatched mutator: {NamespacedName}");
        }

        /// <summary>
        /// Applies supplied metadata used to configure this mutator. If any key of the supplied metadata matches the mutator's NamespacedName,
        /// the metadata under this key is deemed to be for this mutator. If no matching key is found, the supplied metadata is ignored.
        /// </summary>
        /// <param name="metadata">The metadata for the mutator to consume.</param>
        /// <remarks>
        /// Metadata that is not part of the mutator's <see cref="AbstractMutatorSettings.AsMetadata"/> will be deferred until <see cref="MutatorsGameState.LevelReady"/>.
        /// Metadata that is part of the mutator's <see cref="AbstractMutatorSettings.AsMetadata"/> will be applied immediately.
        /// </remarks>
        public void ConsumeMetadata(IDictionary<string, object> metadata)
        {
            if (metadata == null! || metadata.Count == 0) return;

            ConsumeNamedMetadata(metadata);
        }

        private void ConsumeNamedMetadata(IDictionary<string, object> metadata)
        {
            IDictionary<string, object> metaToCheck;

            if (RepoMutators.Settings.ExtendedLogging)
            {
                RepoMutators.Logger.LogDebug($"[{Name}] Consumed Metadata:");
                _metadata.LogMetadata();
            }

            // If metadata is nested under the mutator name, unwrap it
            if (metadata.TryGetValue(NamespacedName, out object? value) && value is IDictionary<string, object> metaForMe)
            {
                metaToCheck = metaForMe;
            }
            else
            {
                RepoMutators.Logger.LogDebug($"[{Name}] No metadata found for '{NamespacedName}'.");
                metaToCheck = new Dictionary<string, object>();
            }

            RepoMutators.Logger.LogDebug($"[{Name}] Metadata: " + string.Join(",", metadata.Keys));
            IDictionary<string, object>? overrides = metadata
                .Get<IDictionary<string, object>>(RepoMutators.MUTATOR_OVERRIDES)?
                .Get<IDictionary<string, object>>(Settings.NamespacedName);

            RepoMutators.Logger.LogDebug($"[{Name}] Overrides: {overrides?.Count.ToString() ?? "null"}");
            if (overrides != null)
            {
                foreach (string key in metaToCheck.Keys.Where(overrides.ContainsKey).ToList())
                {
                    object originalMetaValue = metaToCheck[key];
                    object newValue = overrides[key];

                    Type? origType = originalMetaValue?.GetType();
                    Type? newType = newValue?.GetType();

                    if (newValue.TryCoerce(origType!, out object? coerced))
                    {
                        metaToCheck[key] = coerced!;
                        RepoMutators.Logger.LogDebug($"[{NamespacedName}] Overrode {key} value {originalMetaValue} with {coerced}");
                    }
                    else
                    {
                        RepoMutators.Logger.LogWarning(
                            $"[{NamespacedName}] Failed to override {key}: original type was {(origType == null ? "null" : origType)} but override type was {(newType == null ? "null" : newType)}"
                        );
                    }
                }
            }

            IDictionary<string, object> immediate = new Dictionary<string, object>();
            IDictionary<string, object> immediateKeys = Settings.AsMetadata() ?? new Dictionary<string, object>();

            if (immediateKeys.TryGetValue(NamespacedName, out object? keysNow) && keysNow is IDictionary<string, object> keysForImmediate)
            {
                immediateKeys = keysForImmediate;
            }

            foreach ((string? key, object? val) in metaToCheck)
            {
                // If we have passed the proper game-state (level started), then we can just go
                if (MutatorManager.Instance.GameState >= MutatorsGameState.LevelReady || immediateKeys.ContainsKey(key))
                {
                    immediate[key] = val;
                    RepoMutators.Logger.LogDebug($"Metadata key '{key}' immediate for mutator '{NamespacedName}'.");
                }
                else
                {
                    _pendingDeferredMetadata[key] = val;
                    RepoMutators.Logger.LogDebug($"Metadata key '{key}' deferred for mutator '{NamespacedName}'.");
                }
            }

            if (immediate.Count > 0)
            {
                ApplyMetadata(immediate);
            }
        }

        private void TryApplyDeferredMetadata(MutatorsGameState gameState)
        {
            if (gameState != MutatorsGameState.LevelReady || _pendingDeferredMetadata.Count == 0) return;

            RepoMutators.Logger.LogDebug($"Applying deferred metadata for mutator '{NamespacedName}'");

            ApplyDeferredMetadata(_pendingDeferredMetadata);
        }

        private void ApplyDeferredMetadata(IDictionary<string, object> pendingDeferredMetadata)
        {
            Dictionary<string, object> toApply = new(pendingDeferredMetadata);
            pendingDeferredMetadata.Clear();

            ApplyMetadata(toApply);
        }

        private void ApplyMetadata(IDictionary<string, object> metadataToApply)
        {
            _metadata = _metadata.DeepMergedWith(metadataToApply);

            if (RepoMutators.Settings.ExtendedLogging)
            {
                RepoMutators.Logger.LogDebug("DeepMerged: ");
                _metadata.LogMetadata();
            }

            _onMetadataChangedHooks.ForEach(hook => hook?.Invoke(_metadata));
        }

        private static void TryAddHook(Type type, string methodName, IList<Action> hookList)
        {
            MethodInfo? method = type.GetMethod(
                methodName,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly
            );

            if (method == null) return;

            if (method.GetParameters().Length > 0)
            {
                RepoMutators.Logger.LogWarning($"Lifecycle hook '{methodName}' in type '{type.FullName}' must not have parameters.");
                return;
            }

            if (method.ReturnType != typeof(void))
            {
                RepoMutators.Logger.LogWarning($"Lifecycle hook '{methodName}' in type '{type.FullName}' must return void.");
                return;
            }

            try
            {
                Action action = (Action)Delegate.CreateDelegate(typeof(Action), method);
                hookList.Add(action);
                RepoMutators.Logger.LogDebug($"Lifecycle hook '{methodName}' in type '{type.FullName}' was successfully registered");
            }
            catch (Exception ex)
            {
                RepoMutators.Logger.LogError($"Failed to bind lifecycle hook '{methodName}' in type '{type.FullName}': {ex.Message}");
            }
        }

        private void TryAddMetadataHook(Type type)
        {
            MethodInfo? method = type.GetMethod(
                    "OnMetadataChanged",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly
            );

            if (method == null) return;

            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length != 1)
            {
                RepoMutators.Logger.LogWarning($"Lifecycle hook 'OnMetadataChanged' in type '{type.FullName}' must have one parameter of type IDictionary<string, object>.");
                return;
            }

            if (method.ReturnType != typeof(void))
            {
                RepoMutators.Logger.LogWarning($"Lifecycle hook 'OnMetadataChanged' in type '{type.FullName}' must return void.");
                return;
            }

            if (!typeof(IDictionary<string, object>).IsAssignableFrom(parameters[0].ParameterType))
            {
                RepoMutators.Logger.LogWarning($"Lifecycle hook 'OnMetadataChanged' in type '{type.FullName}' must take a parameter of type IDictionary<string, object> or a compatible type.");
                return;
            }

            try
            {
                Action<IDictionary<string, object>> action = (Action<IDictionary<string, object>>)Delegate.CreateDelegate(typeof(Action<IDictionary<string, object>>), method);
                _onMetadataChangedHooks.Add(action);
                RepoMutators.Logger.LogDebug($"Lifecycle hook 'OnMetadataChanged' in type '{type.FullName}' was successfully registered");
            }
            catch (Exception ex)
            {
                RepoMutators.Logger.LogError($"Failed to bind lifecycle hook 'OnMetadataChanged' in type '{type.FullName}': {ex.Message}");
            }
        }
    }
}
