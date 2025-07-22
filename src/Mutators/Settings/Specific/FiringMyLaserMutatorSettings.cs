using System.Collections.Generic;
using BepInEx.Configuration;

namespace Mutators.Settings.Specific
{
    /// <summary>
    /// Settings for the Firing My Laser mutator.
    /// </summary>
    public class FiringMyLaserMutatorSettings : GenericMutatorSettings
    {
        /// <summary>
        /// Metadata key for the manual laser action cooldown.
        /// </summary>
        public const string LaserActionCooldownKey = "laser-action-cooldown";

        /// <summary>
        /// Metadata key for the laser damage dealt to enemies.
        /// </summary>
        public const string LaserActionEnemyDamageKey = "laser-enemy-damage";
        
        /// <summary>
        /// Metadata key for whether the manual laser action is enabled.
        /// </summary>
        public const string LaserActionEnabledKey = "laser-action-enabled";
        
        /// <summary>
        /// Metadata key for whether the laser is automatically fired when getting hurt.
        /// </summary>
        public const string LaserOnHurtEnabledKey = "laser-on-hurt-enabled";

        private readonly ConfigEntry<int> _laserActionCooldown;
        private readonly ConfigEntry<int> _laserActionEnemyDamage;
        private readonly ConfigEntry<bool> _laserActionEnabled;
        private readonly ConfigEntry<bool> _laserOnHurtEnabled;
        
        /// <summary>
        /// The amount of seconds before the manual laser action can be used again.
        /// </summary>
        public int LaserActionCooldown => GetClampedRuntimeOverride(LaserActionCooldownKey, _laserActionCooldown);
        
        /// <summary>
        /// The amount of damage the laser deals to enemies per tick.
        /// </summary>
        public int LaserActionEnemyDamage => GetClampedRuntimeOverride(LaserActionEnemyDamageKey, _laserActionEnemyDamage);
        
        /// <summary>
        /// Whether the manual laser action is enabled.
        /// </summary>
        public bool LaserActionEnabled => GetRuntimeOverride(LaserActionEnabledKey, _laserActionEnabled.Value);
        
        /// <summary>
        /// Whether the laser is automatically fired when getting hurt.
        /// </summary>
        public bool LaserOnHurtEnabled => GetRuntimeOverride(LaserOnHurtEnabledKey, _laserOnHurtEnabled.Value);

        internal FiringMyLaserMutatorSettings(string name, string description, ConfigFile config) : base(MyPluginInfo.PLUGIN_GUID, name, description, config)
        {
            _laserActionEnabled = config.Bind(
            GetSection(name),
            "Allow manual laser action",
            true,
            "If true, players can manually use their laser action while the cooldown is over. Otherwise, only use the laser when getting hit"
            );
            
            _laserOnHurtEnabled = config.Bind(
                GetSection(name),
                "Fire laser on hurt",
                true,
                "If true, players will automatically fire their laser when getting hurt."
            );

            _laserActionCooldown = config.Bind(
            GetSection(name),
            "Laser action cooldown",
            60,
            new ConfigDescription(
                $"The amount of seconds before the laser special action can be used again.",
                new AcceptableValueRange<int>(5, 10000))
            );

            _laserActionEnemyDamage = config.Bind(
            GetSection(name),
            "Laser action enemy damage",
            30,
            new ConfigDescription(
                $"The amount of damage the laser special action deals to enemies per tick.",
                new AcceptableValueRange<int>(10, 200))
            );
        }

        /// <inheritdoc cref="AbstractMutatorSettings.CreateMetadata"/>
        /// <returns>A dictionary holding <c>laser-action-cooldown</c> and <c>laser-on-hurt-enabled</c></returns>
        protected override IDictionary<string, object> CreateMetadata()
        {
            return new Dictionary<string, object>
            {
                { LaserActionEnabledKey, LaserActionEnabled },
                { LaserOnHurtEnabledKey, LaserOnHurtEnabled }
            };
        }
    }
}
