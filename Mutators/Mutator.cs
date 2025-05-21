using HarmonyLib;
using Mutators.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Mutators.Mutators
{
    public class Mutator : IMutator
    {
        private readonly Harmony _harmony;
        private readonly IList<Type> _patches;
        private readonly IList<Func<bool>> _conditions;
        public string Name => Settings.MutatorName;
        public string Description => Settings.MutatorDescription;
        public bool Active { get; private set; }
        public AbstractMutatorSettings Settings { get; private set; }
        public IReadOnlyList<Func<bool>> Conditions => new ReadOnlyCollection<Func<bool>>(_conditions);
        public IReadOnlyList<Type> Patches => new ReadOnlyCollection<Type>(_patches);

        public Mutator(AbstractMutatorSettings settings, IList<Type> patches, IList<Func<bool>> conditions = null!)
        {
            Settings = settings;
            _harmony = new Harmony($"{MyPluginInfo.PLUGIN_GUID}-{settings.MutatorName}");
            _patches = patches ?? [];
            _conditions = conditions ?? [];
        }

        public Mutator(AbstractMutatorSettings settings, Type patch, IList<Func<bool>> conditions = null!) : this(settings, [patch], conditions)
        {
            
        }

        public void Patch()
        {
            RepoMutators.Logger.LogDebug($"{Name} active: {Active}");
            if (Active) return;

            RepoMutators.Logger.LogDebug($"About to apply {_patches.Count} patches for {Name}");

            Active = true;
            foreach (Type patch in _patches)
            {
                _harmony.PatchAll(patch);
                RepoMutators.Logger.LogDebug($"Applied patch: {patch.Name}");
            }
        }

        public void Unpatch()
        {
            if (!Active) return;

            _harmony.UnpatchSelf();
            Active = false;
            RepoMutators.Logger.LogDebug($"Unpatched mutator: {Name}");
        }
    }
}
