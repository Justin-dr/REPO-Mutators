using BepInEx.Configuration;

namespace Mutators.Settings.Specific
{
    /// <summary>
    /// Settings for the Sealed Away mutator.
    /// </summary>
    public class SealedAwayMutatorSettings : EnemyDisablingMutatorSettings
    {
        /// <summary>
        /// Metadata key for the maximum amount of monsters spawned by broken valuables.
        /// </summary>
        public const string MaximumMonsterSpawnsKey = "maximum-monster-spawns";

        /// <summary>
        /// Metadata key for the chance that breaking a valuable spawns a monster.
        /// </summary>
        public const string MonsterSpawnChanceKey = "monster-spawn-chance";

        private readonly ConfigEntry<int> _maximumMonsterSpawns;
        private readonly ConfigEntry<float> _monsterSpawnChance;

        /// <summary>
        /// The maximum amount of extra monsters spawned per level.
        /// </summary>
        public int MaximumMonsterSpawns => GetClampedRuntimeOverride(MaximumMonsterSpawnsKey, _maximumMonsterSpawns); 

        /// <summary>
        /// The percentage chance that breaking a valuable spawns a monster.
        /// </summary>
        public float MonsterSpawnChance => GetClampedRuntimeOverride(MonsterSpawnChanceKey, _monsterSpawnChance);

        internal SealedAwayMutatorSettings(string name, string description, ConfigFile config) : base(MyPluginInfo.PLUGIN_GUID, name, description, config, "Voodoo", "Shadow Child")
        {
            _maximumMonsterSpawns = config.Bind(
            GetSection(name),
            "Maximum monster spawns",
            5,
            new ConfigDescription(
                $"Maximum amount of extra monsters that can be spawned by the {name} Mutator per level.",
                new AcceptableValueRange<int>(1, 15)
                )
            );

            _monsterSpawnChance = config.Bind<float>(
            GetSection(name),
            "Monster spawn chance",
            65,
            new ConfigDescription(
                $"The chance that a monster is spawned when breaking a valuable when the {name} Mutator is active.",
                new AcceptableValueRange<float>(0, 100)
                )
            );
        }
    }
}
