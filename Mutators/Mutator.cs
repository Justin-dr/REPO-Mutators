using HarmonyLib;
using Mutators.Settings;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Mutators.Mutators
{
    public class Mutator : IMutator
    {
        private readonly Harmony _harmony;
        private readonly IList<Type> _patches;
        private readonly IList<Func<bool>> _conditions;

        // HOOKS
        private readonly IList<Action> _beforePatchAllHooks = [];
        private readonly IList<Action> _afterPatchAllHooks = [];
        private readonly IList<Action> _beforeUnpatchAllHooks = [];
        private readonly IList<Action> _afterUnpatchAllHooks = [];

        // PROPS
        public string Name => Settings.MutatorName;
        public string Description => Settings.MutatorDescription;
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
            HasSpecialAction = specialActionOverlay;

            foreach (Type patch in _patches)
            {
                TryAddHook(patch, "BeforePatchAll", _beforePatchAllHooks);
                TryAddHook(patch, "AfterPatchAll", _afterPatchAllHooks);
                TryAddHook(patch, "BeforeUnpatchAll", _beforeUnpatchAllHooks);
                TryAddHook(patch, "AfterUnpatchAll", _afterUnpatchAllHooks);
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

            _beforeUnpatchAllHooks.ForEach(action => action?.Invoke());

            _harmony.UnpatchSelf();
            Active = false;

            _afterUnpatchAllHooks.ForEach(action => action?.Invoke());
            RepoMutators.Logger.LogDebug($"Unpatched mutator: {Name}");
        }

        private static void TryAddHook(Type type, string methodName, IList<Action> hookList)
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
    }
}
