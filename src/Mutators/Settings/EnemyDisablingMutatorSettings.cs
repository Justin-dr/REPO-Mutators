using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;

namespace Mutators.Settings
{
    /// <summary>
    /// Subclass of <see cref="GenericMutatorSettings"/> which additionally takes in a list of enemy names that cannot be
    /// spawned while the mutator is active.
    /// </summary>
    public class EnemyDisablingMutatorSettings : GenericMutatorSettings
    {
        /// <summary>
        /// Metadata key for the excluded enemies.
        /// </summary>
        public const string ExcludedEnemiesKey = "excluded-enemies";

        private readonly ConfigEntry<string> _excludedEnemies;
        private IList<string> _excludedEnemiesList = [];

        /// <summary>
        /// List of enemy names that cannot be spawned while the mutator is active.
        /// </summary>
        public IList<string> ExcludedEnemies => GetRuntimeOverrideList(ExcludedEnemiesKey, _excludedEnemiesList);

        /// <summary>
        /// Initializes settings for a mutator that disables specific enemy spawns.
        /// </summary>
        /// <param name="namespace">The namespace used to make the mutator's slug unique.</param>
        /// <param name="name">The display name of the mutator.</param>
        /// <param name="description">The base description shown for the mutator.</param>
        /// <param name="weight">The default weighted selection value for the mutator.</param>
        /// <param name="config">The config file used to bind the mutator settings.</param>
        /// <param name="defaultDisabledEnemies">The enemy names disabled by default.</param>
        public EnemyDisablingMutatorSettings(string @namespace, string name, string description, int weight, ConfigFile config, params string[] defaultDisabledEnemies) : base(@namespace, name, description, weight, config)
        {
            _excludedEnemies = config.Bind(
                GetSection(name),
                "Excluded enemies",
                string.Join(", ", defaultDisabledEnemies),
                $"Enemies that cannot be spawned by the {name} Mutator. (Comma separated e.g. Apex Predator,Huntsman)"
            );

            _excludedEnemies.SettingChanged += (_, _) => CacheEnemies();
            CacheEnemies();
        }

        /// <summary>
        /// Initializes settings for a mutator that disables specific enemy spawns using the default selection weight.
        /// </summary>
        /// <param name="namespace">The namespace used to make the mutator's slug unique.</param>
        /// <param name="name">The display name of the mutator.</param>
        /// <param name="description">The base description shown for the mutator.</param>
        /// <param name="config">The config file used to bind the mutator settings.</param>
        /// <param name="defaultDisabledEnemies">The enemy names disabled by default.</param>
        public EnemyDisablingMutatorSettings(string @namespace, string name, string description, ConfigFile config, params string[] defaultDisabledEnemies) : this(@namespace, name, description, 100, config, defaultDisabledEnemies)
        {

        }

        internal void CacheEnemies()
        {
            _excludedEnemiesList = _excludedEnemies.Value.Split(",")
                .Select(value => value.Trim()).ToList();
        }
    }
}
