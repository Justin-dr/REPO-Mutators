using HarmonyLib;
using Mutators.Enums;
using Mutators.Extensions;
using Mutators.Managers;
using Mutators.Settings;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace Mutators.Mutators
{
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
        public string Name => Settings.MutatorName;
        public string Description => Settings.MutatorDescription;
        public IReadOnlyDictionary<string, object> Metadata => new ReadOnlyDictionary<string, object>(_metadata);
        public bool Active { get; private set; }
        public bool HasSpecialAction { get; private set; }
        public AbstractMutatorSettings Settings { get; private set; }
        public IReadOnlyList<Func<bool>> Conditions => new ReadOnlyCollection<Func<bool>>(_conditions);
        public IReadOnlyList<Type> Patches => new ReadOnlyCollection<Type>(_patches);

        public Mutator(AbstractMutatorSettings settings, IList<Type> patches, IList<Func<bool>> conditions = null!, bool specialActionOverlay = false)
        {
            Settings = settings;
            _harmony = new Harmony($"{MyPluginInfo.PLUGIN_GUID}-{settings.MutatorName}");
            _patches = patches ?? [];
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

        public Mutator(AbstractMutatorSettings settings, Type patch, IList<Func<bool>> conditions = null!, bool specialActionOverlay = false) : this(settings, [patch], conditions, specialActionOverlay)
        {
            
        }

        public void Patch()
        {
            RepoMutators.Logger.LogDebug($"{Name} active: {Active}");
            if (Active) return;

            RepoMutators.Logger.LogDebug($"About to apply {_patches.Count} patches for {Name}");

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
            RepoMutators.Logger.LogDebug($"Unpatched mutator: {Name}");
        }

        public void ConsumeMetadata(IDictionary<string, object> metadata)
        {
            if (metadata == null || metadata.Count == 0) return;

            ConsumeNamedMetadata(metadata);
        }

        protected void ConsumeNamedMetadata(IDictionary<string, object> metadata)
        {
            IDictionary<string, object> metaToCheck = metadata;

            // If metadata is nested under the mutator name, unwrap it
            if (metadata.TryGetValue(Settings.MutatorName, out object? value) && value is IDictionary<string, object> metaForMe)
            {
                metaToCheck = metaForMe;
            }

            RepoMutators.Logger.LogInfo($"[{Name}] Metadata: " + string.Join(",", metadata.Keys));
            IDictionary<string, object>? overrides = metadata.Get<IDictionary<string, object>>(RepoMutators.MUTATOR_OVERRIDES)?.Get<IDictionary<string, object>>(Settings.MutatorName);

            RepoMutators.Logger.LogInfo($"[{Name}] Overrides: {overrides?.Count.ToString() ?? "null"}");
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
                        RepoMutators.Logger.LogDebug($"[{Name}] Overrode {key} value {originalMetaValue} with {coerced}");
                    }
                    else
                    {
                        RepoMutators.Logger.LogWarning(
                            $"[{Name}] Failed to override {key}: original type was {(origType == null ? "null" : origType)} but override type was {(newType == null ? "null" : newType)}"
                        );
                    }
                }
            }

            IDictionary<string, object> immediate = new Dictionary<string, object>();
            IDictionary<string, object> _immediateKeys = Settings.AsMetadata() ?? new Dictionary<string, object>();

            if (_immediateKeys.TryGetValue(Settings.MutatorName, out object? keysNow) && keysNow is IDictionary<string, object> keysForImmediate)
            {
                _immediateKeys = keysForImmediate;
            }

            foreach (var (key, val) in metaToCheck)
            {
                // If we have passed the proper gamestate then we can just go
                if (MutatorManager.Instance.GameState > MutatorsGameState.None || _immediateKeys.ContainsKey(key))
                {
                    immediate[key] = val;
                    RepoMutators.Logger.LogDebug($"Metadata key '{key}' immediate for mutator '{Name}'.");
                }
                else
                {
                    _pendingDeferredMetadata[key] = val;
                    RepoMutators.Logger.LogDebug($"Metadata key '{key}' deferred for mutator '{Name}'.");
                }
            }

            if (immediate.Count > 0)
            {
                ApplyMetadata(immediate);
            }
        }

        protected void TryApplyDeferredMetadata(MutatorsGameState gameState)
        {
            if (gameState != MutatorsGameState.LevelGenerated || _pendingDeferredMetadata.Count == 0) return;

            RepoMutators.Logger.LogDebug($"Applying deferred metadata for mutator '{Name}'");

            ApplyDeferredMetadata(_pendingDeferredMetadata);
        }

        protected void ApplyDeferredMetadata(IDictionary<string, object> pendingDeferredMetadata)
        {
            var toApply = new Dictionary<string, object>(pendingDeferredMetadata);
            pendingDeferredMetadata.Clear();

            ApplyMetadata(toApply);
        }

        protected void ApplyMetadata(IDictionary<string, object> metadataToApply)
        {
            _metadata = _metadata.DeepMergedWith(metadataToApply);

            RepoMutators.Logger.LogInfo("DeepMerged: ");
            foreach (var item in _metadata)
            {
                RepoMutators.Logger.LogInfo($"{item.Key}: {item.Value}");
            }

            _onMetadataChangedHooks.ForEach(hook => hook?.Invoke(_metadata));
        }

        protected static void TryAddHook(Type type, string methodName, IList<Action> hookList)
        {
            var method = type.GetMethod(
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
                var action = (Action)Delegate.CreateDelegate(typeof(Action), method);
                hookList.Add(action);
                RepoMutators.Logger.LogDebug($"Lifecycle hook '{methodName}' in type '{type.FullName}' was succesfully registered");
            }
            catch (Exception ex)
            {
                RepoMutators.Logger.LogError($"Failed to bind lifecycle hook '{methodName}' in type '{type.FullName}': {ex.Message}");
            }
        }

        protected void TryAddMetadataHook(Type type)
        {
            var method = type.GetMethod(
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
                var action = (Action<IDictionary<string, object>>)Delegate.CreateDelegate(typeof(Action<IDictionary<string, object>>), method);
                _onMetadataChangedHooks.Add(action);
                RepoMutators.Logger.LogDebug($"Lifecycle hook 'OnMetadataChanged' in type '{type.FullName}' was succesfully registered");
            }
            catch (Exception ex)
            {
                RepoMutators.Logger.LogError($"Failed to bind lifecycle hook 'OnMetadataChanged' in type '{type.FullName}': {ex.Message}");
            }
        }
    }
}
