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
        public string Name { get; private set; }
        public bool Active { get; private set; }
        public AbstractMutatorSettings Settings { get; private set; }
        public IReadOnlyList<Func<bool>> Conditions => new ReadOnlyCollection<Func<bool>>(_conditions);
        public IReadOnlyList<Type> Patches => new ReadOnlyCollection<Type>(_patches);

        public Mutator(string name, IList<Type> patches, AbstractMutatorSettings settings, IList<Func<bool>> conditions = null!)
        {
            Name = name;
            Settings = settings;
            _harmony = new Harmony($"{MyPluginInfo.PLUGIN_GUID}-{name}");
            _patches = patches ?? [];
            _conditions = conditions ?? [];
        }

        public Mutator(string name, Type patch, AbstractMutatorSettings settings, IList<Func<bool>> conditions = null!) : this(name, [patch], settings, conditions)
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
