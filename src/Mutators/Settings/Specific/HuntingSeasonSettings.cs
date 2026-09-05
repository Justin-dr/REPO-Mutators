using System;
using System.Collections.Generic;
using BepInEx.Configuration;

namespace Mutators.Settings.Specific
{
    /// <summary>
    /// Settings for the Hunting Season mutator.
    /// </summary>
    public class HuntingSeasonSettings : EnemyDisablingMutatorSettings
    {
        private readonly ConfigEntry<int> _enemyRespawnTime;
        
        /// <summary>
        /// Metadata key for enemy respawn time.
        /// </summary>
        public const string RespawnTime = "respawn-time";
        
        internal event EventHandler EnemyRespawnTimeChanged
        {
            add => _enemyRespawnTime.SettingChanged += value;
            remove => _enemyRespawnTime.SettingChanged -= value;
        }

        /// <summary>
        /// The time in seconds at which enemy respawn time is capped.
        /// </summary>
        public int EnemyRespawnTime => GetClampedRuntimeOverride(RespawnTime, _enemyRespawnTime);

        internal HuntingSeasonSettings(string @namespace, string name, string description, int weight, ConfigFile config, params string[] defaultDisabledEnemies) : base(@namespace, name, description, weight, config, defaultDisabledEnemies)
        {
            _enemyRespawnTime = CreateRespawnTimeConfigEntry(config, name);
        }

        internal HuntingSeasonSettings(string @namespace, string name, string description, ConfigFile config, params string[] defaultDisabledEnemies) : base(@namespace, name, description, config, defaultDisabledEnemies)
        {
            _enemyRespawnTime = CreateRespawnTimeConfigEntry(config, name);
        }

        private ConfigEntry<int> CreateRespawnTimeConfigEntry(ConfigFile config, string name)
        {
            return config.Bind(
                GetSection(name),
                "Enemy Respawn Time",
                10,
                new ConfigDescription(
                    $"The time it takes before enemies respawn during the {name} Mutator. If another source (mod, base game) sets this to a lower value, the lower value will be used.",
                    new AcceptableValueRange<int>(1, 120))
            );
        }
        
        /// <inheritdoc cref="AbstractMutatorSettings.CreateMetadata"/>
        /// <returns>A dictionary holding <c>respawn-time</c></returns>
        protected override IDictionary<string, object> CreateMetadata()
        {
            return new Dictionary<string, object>(1)
            {
                { RespawnTime, EnemyRespawnTime }
            };
        }
    }
}