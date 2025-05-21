using BepInEx.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Mutators.Settings
{
    public class EnemyDisablingMutatorSettings : GenericMutatorSettings
    {
        private readonly ConfigEntry<string> _excludedEnemies;
        public IList<string> ExcludedEnemies { get; private set; } = [];
        public EnemyDisablingMutatorSettings(string name, string description, ConfigFile config, params string[] defaultDisabledEnemies) : base(name, description, config)
        {
            _excludedEnemies = config.Bind(
            GetSection(name),
            "Excluded enemies",
            string.Join(", ", defaultDisabledEnemies),
            $"Enemies that cannot be spawned by the {name} Mutator. (Comma separated e.g. Apex Predator,Huntsman)"
            );

            CacheEnemies();
        }

        internal void CacheEnemies()
        {
            ExcludedEnemies = _excludedEnemies.Value.Split(",")
                .Select(value => value.Trim()).ToList();
        }
    }
}
