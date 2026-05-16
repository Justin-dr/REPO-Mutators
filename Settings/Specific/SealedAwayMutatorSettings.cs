using BepInEx.Configuration;

namespace Mutators.Settings.Specific
{
    public class SealedAwayMutatorSettings : EnemyDisablingMutatorSettings
    {
        public const string MaximumMonsterSpawnsKey = "maximumMonsterSpawns";
        public const string MonsterSpawnChanceKey = "monsterSpawnChance";

        private readonly ConfigEntry<int> _maximumMonsterSpawns;
        private readonly ConfigEntry<float> _monsterSpawnChance;
        public int MaximumMonsterSpawns => GetRuntimeOverride(MaximumMonsterSpawnsKey, _maximumMonsterSpawns.Value);
        public float MonsterSpawnChance => GetRuntimeOverride(MonsterSpawnChanceKey, _monsterSpawnChance.Value);

        internal SealedAwayMutatorSettings(string name, string description, ConfigFile config) : base(name, description, config, "Voodoo", "Shadow Child")
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
            10,
            new ConfigDescription(
                $"The chance that a monster is spawned when breaking a valuable when the {name} Mutator is active.",
                new AcceptableValueRange<float>(0, 100)
                )
            );
        }
    }
}
