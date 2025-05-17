using BepInEx.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Mutators.Settings.Specific
{
    public class ThereCanOnlyBeOneMutatorSettings : GenericMutatorSettings
    {
        private readonly ConfigEntry<uint> _groupSpawnsThreshold;
        private readonly ConfigEntry<string> _excludedEnemies;
        public uint GroupSpawnsThreshold => _groupSpawnsThreshold.Value;
        public IList<string> ExcludedEnemies { get; set; } = [];
        internal ThereCanOnlyBeOneMutatorSettings(string name, ConfigFile config) : base(name, config)
        {
            _groupSpawnsThreshold = config.Bind<uint>(
            GetSection(name),
            "Group spawn minimum level",
            3,
            $"The minimum level from which enemy groups (e.g. 3 mentalists or 10 gnomes) can be spawned by the {name} Mutator."
            );

            _excludedEnemies = config.Bind(
            GetSection(name),
            "Excluded enemies",
            string.Empty,
            $"Enemies that cannot be spawned by the {name} Mutator. (Comma separated e.g. Apex Predator,Huntsman)"
            );

            Cache();
        }

        internal void Cache()
        {
            ExcludedEnemies = _excludedEnemies.Value.Split(",")
                .Select(value => value.Trim()).ToList();
        }
    }
}
