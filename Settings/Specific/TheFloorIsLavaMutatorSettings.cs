using BepInEx.Configuration;
using Mutators.Extensions;
using Mutators.Mutators.Patches;
using System;
using System.Collections.Generic;

namespace Mutators.Settings.Specific
{
    public class TheFloorIsLavaMutatorSettings : GenericMutatorSettings, ILevelRemovingMutatorSettings
    {
        private readonly ConfigEntry<bool> _usePercentageDamage;
        private readonly ConfigEntry<bool> _allowCustomLevels;
        private readonly ConfigEntry<bool> _disableEnemies;
        public int DamagePerTick { get; private set; }
        public int ImmunePlayerCount { get; private set; }
        public bool UsePercentageDamage => _usePercentageDamage.Value;
        public bool AllowCustomLevels => _allowCustomLevels.Value;
        public bool DisableEnemies => _disableEnemies.Value;

        public IList<string> ExcludedLevels => [];

        internal TheFloorIsLavaMutatorSettings(string name, string description, ConfigFile config) : base(name, description, config)
        {
            ConfigEntry<int> _damagePerTick = config.Bind(
            GetSection(name),
            "Damage",
            1,
            $"The amount of damage that players receive per second when standing on floor tiles."
            );

            _usePercentageDamage = config.Bind(
            GetSection(name),
            "Use percentage damage",
            false,
            $"If true, players will receive percentage max health damage instead of a flat amount."
            );

            ConfigEntry<int>  _immunePlayerCount = config.Bind(
            GetSection(name),
            "Immune player count",
            0,
            $"The amount of players that are immune to {name} damage."
            );

            _allowCustomLevels = config.Bind(
            GetSection(name),
            "Allow custom levels",
            true,
            $"If false, custom levels cannot be picked while the {name} Mutator is active."
            );

            _disableEnemies = config.Bind(
            GetSection(name),
            "Disable enemies",
            false,
            $"If true, no enemies will spawn while the {name} Mutator is active."
            );

            DamagePerTick = Math.Clamp(_damagePerTick.Value, 1, int.MaxValue);
            ImmunePlayerCount = Math.Clamp(_immunePlayerCount.Value, 0, int.MaxValue);
        }

        public override IDictionary<string, object>? AsMetadata()
        {
            IDictionary<string, object> metadata = new Dictionary<string, object>
            {
                { TheFloorIsLavaPatch.Damage, DamagePerTick },
                { TheFloorIsLavaPatch.UsePercentageDamage, UsePercentageDamage },
            };

            return metadata.WithMutator(MutatorName);
        }
    }
}
