using BepInEx.Configuration;

namespace Mutators.Settings.Specific
{
    public class ThereCanOnlyBeOneMutatorSettings : EnemyDisablingMutatorSettings
    {
        private readonly ConfigEntry<uint> _groupSpawnsThreshold;
        public uint GroupSpawnsThreshold => _groupSpawnsThreshold.Value;
        internal ThereCanOnlyBeOneMutatorSettings(string name, string description, ConfigFile config) : base(name, description, config)
        {
            _groupSpawnsThreshold = config.Bind<uint>(
            GetSection(name),
            "Group spawn minimum level",
            8,
            $"The minimum level from which enemy groups (e.g. 3 mentalists or 10 gnomes) can be spawned by the {name} Mutator."
            );
        }
    }
}
