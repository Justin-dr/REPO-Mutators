using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using Mutators.Extensions;
using Mutators.Mutators.Patches;

namespace Mutators.Settings.Specific
{
    /// <summary>
    /// Settings for the The Floor Is Lava mutator.
    /// </summary>
    public class TheFloorIsLavaMutatorSettings : GenericMutatorSettings, ILevelRemovingMutatorSettings
    {
        /// <summary>
        /// Metadata key for the amount of players immune to lava damage.
        /// </summary>
        public const string ImmunePlayerCountKey = "immune-player-amount";

        /// <summary>
        /// Metadata key for whether custom levels can be selected.
        /// </summary>
        public const string AllowCustomLevelsKey = "allow-custom-levels";

        /// <summary>
        /// Metadata key for whether enemies should be disabled.
        /// </summary>
        public const string DisableEnemiesKey = "disable-enemies";

        private readonly ConfigEntry<int> _immunePlayerCount;
        private readonly ConfigEntry<bool> _usePercentageDamage;
        private readonly ConfigEntry<bool> _allowCustomLevels;
        private readonly ConfigEntry<bool> _disableEnemies;
        private readonly ConfigEntry<float> _reviveImmunityDuration;
        /// <summary>
        /// The damage players receive per tick while standing on the floor.
        /// </summary>
        public int DamagePerTick { get; private set; }

        /// <summary>
        /// The amount of players immune to lava damage.
        /// </summary>
        public int ImmunePlayerCount => GetClampedRuntimeOverride(ImmunePlayerCountKey, _immunePlayerCount);

        /// <summary>
        /// Whether lava damage is applied as a percentage of maximum health.
        /// </summary>
        public bool UsePercentageDamage => _usePercentageDamage.Value;

        /// <summary>
        /// Whether custom levels can be selected while the The Floor Is Lava mutator is active.
        /// </summary>
        public bool AllowCustomLevels => GetRuntimeOverride(AllowCustomLevelsKey, _allowCustomLevels.Value);

        /// <summary>
        /// Whether enemies are disabled while the The Floor Is Lava mutator is active.
        /// </summary>
        public bool DisableEnemies => GetRuntimeOverride(DisableEnemiesKey, _disableEnemies.Value);

        /// <summary>
        /// The time in seconds for which players are immune to lava damage after reviving.
        /// </summary>
        public float ReviveImmunityDuration => _reviveImmunityDuration.Value;

        /// <summary>
        /// List of level names that should be excluded from the The Floor Is Lava mutator.
        /// </summary>
        public IList<string> ExcludedLevels => [];

        internal TheFloorIsLavaMutatorSettings(string name, string description, ConfigFile config) : base(MyPluginInfo.PLUGIN_GUID, name, description, 0, config)
        {
            ConfigEntry<int> _damagePerTick = config.BindPositive(
                GetSection(name),
                "Damage",
                1,
                "The amount of damage that players receive per second when standing on floor tiles."
            );

            _usePercentageDamage = config.Bind(
                GetSection(name),
                "Use percentage damage",
                false,
                "If true, players will receive percentage max health damage instead of a flat amount."
            );

            _immunePlayerCount = config.Bind(
                GetSection(name),
                "Immune player count",
                0,
                new ConfigDescription(
                    $"The amount of players that are immune to {name} damage.",
                    new AcceptableValueRange<int>(0, 20))
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

            _reviveImmunityDuration = config.Bind(
                GetSection(name),
                "Revive immunity duration",
                20f,
                $"The time in seconds for which players are immune to lava damage after reviving while the {name} Mutator is active."
            );

            DamagePerTick = Math.Clamp(_damagePerTick.Value, 1, int.MaxValue);
        }

        /// <inheritdoc cref="AbstractMutatorSettings.CreateMetadata"/>
        /// <returns>A dictionary holding <c>damage</c>, <c>is-percentage-damage</c>, and <c>revive-immunity-duration</c></returns>
        protected override IDictionary<string, object>? CreateMetadata()
        {
            return new Dictionary<string, object>
            {
                { TheFloorIsLavaPatch.Damage, DamagePerTick },
                { TheFloorIsLavaPatch.UsePercentageDamage, UsePercentageDamage },
                { TheFloorIsLavaPatch.RevivalImmunityDuration, ReviveImmunityDuration }
            };
        }
    }
}
