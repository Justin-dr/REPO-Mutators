using HarmonyLib;
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
        public uint Weight { get; private set; }
        public IReadOnlyList<Func<bool>> Conditions => new ReadOnlyCollection<Func<bool>>(_conditions);
        public IReadOnlyList<Type> Patches => new ReadOnlyCollection<Type>(_patches);

        public Mutator(string name, IList<Type> patches, uint weight, IList<Func<bool>> conditions = null!)
        {
            Name = name;
            Weight = weight;
            _harmony = new Harmony($"{MyPluginInfo.PLUGIN_GUID}-{name}");
            _patches = patches ?? [];
            _conditions = conditions ?? [];
        }

        public Mutator(string name, Type patch, uint weight, IList<Func<bool>> conditions = null!) : this(name, [patch], weight, conditions)
        {
            
        }

        public void Patch()
        {
            RepoMutators.Logger.LogInfo($"{Name} active: {Active}");
            if (Active) return;

            RepoMutators.Logger.LogInfo($"About to apply {_patches.Count} patches for {Name}");

            Active = true;
            foreach (Type patch in _patches)
            {
                _harmony.PatchAll(patch);
                RepoMutators.Logger.LogInfo($"Applied patch");
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
