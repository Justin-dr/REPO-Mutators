using System.Collections.Generic;
using BepInEx.Configuration;
using Mutators.Extensions;

namespace Mutators.Settings.Specific
{
    /// <summary>
    /// Settings for the Out With a Bang mutator.
    /// </summary>
    public class OutWithABangMutatorSettings : GenericMutatorSettings
    {
        /// <summary>
        /// Metadata key for the tier 1 enemy explosion radius.
        /// </summary>
        public const string Tier1Radius = "tier-1-enemy-explosion-radius";

        /// <summary>
        /// Metadata key for the tier 1 enemy explosion damage.
        /// </summary>
        public const string Tier1Damage = "tier-1-enemy-explosion-damage";

        /// <summary>
        /// Metadata key for the tier 2 enemy explosion radius.
        /// </summary>
        public const string Tier2Radius = "tier-2-enemy-explosion-radius";

        /// <summary>
        /// Metadata key for the tier 2 enemy explosion damage.
        /// </summary>
        public const string Tier2Damage = "tier-2-enemy-explosion-damage";

        /// <summary>
        /// Metadata key for the tier 3 enemy explosion radius.
        /// </summary>
        public const string Tier3Radius = "tier-3-enemy-explosion-radius";

        /// <summary>
        /// Metadata key for the tier 3 enemy explosion damage.
        /// </summary>
        public const string Tier3Damage = "tier-3-enemy-explosion-damage";

        private readonly ConfigEntry<float> _tier1ExplosionRadius;
        private readonly ConfigEntry<int> _tier1ExplosionDamage;
        private readonly ConfigEntry<float> _tier2ExplosionRadius;
        private readonly ConfigEntry<int> _tier2ExplosionDamage;
        private readonly ConfigEntry<float> _tier3ExplosionRadius;
        private readonly ConfigEntry<int> _tier3ExplosionDamage;

        /// <summary>
        /// The explosion radius for tier 1 enemies.
        /// </summary>
        public float Tier1ExplosionRadius => _tier1ExplosionRadius.Value;

        /// <summary>
        /// The explosion damage for tier 1 enemies.
        /// </summary>
        public int Tier1ExplosionDamage => _tier1ExplosionDamage.Value;

        /// <summary>
        /// The explosion radius for tier 2 enemies.
        /// </summary>
        public float Tier2ExplosionRadius => _tier2ExplosionRadius.Value;

        /// <summary>
        /// The explosion damage for tier 2 enemies.
        /// </summary>
        public int Tier2ExplosionDamage => _tier2ExplosionDamage.Value;

        /// <summary>
        /// The explosion radius for tier 3 enemies.
        /// </summary>
        public float Tier3ExplosionRadius => _tier3ExplosionRadius.Value;

        /// <summary>
        /// The explosion damage for tier 3 enemies.
        /// </summary>
        public int Tier3ExplosionDamage => _tier3ExplosionDamage.Value;

        internal OutWithABangMutatorSettings(string name, string description, ConfigFile config) : base(MyPluginInfo.PLUGIN_GUID, name, description, config)
        {
            _tier1ExplosionRadius = config.Bind(
            GetSection(name),
            "Tier 1 Enemy Explosion Radius",
            1f,
            $"The on-death explosion radius of Tier 1 enemies during the {name} Mutator."
            );

            _tier1ExplosionDamage = config.BindPositive(
            GetSection(name),
            "Tier 1 Enemy Explosion Damage",
            50,
            $"The on-death explosion damage of Tier 1 enemies during the {name} Mutator."
            );

            _tier2ExplosionRadius = config.Bind(
            GetSection(name),
            "Tier 2 Enemy Explosion Radius",
            2f,
            $"The on-death explosion radius of Tier 2 enemies during the {name} Mutator."
            );

            _tier2ExplosionDamage = config.BindPositive(
            GetSection(name),
            "Tier 2 Enemy Explosion Damage",
            100,
            $"The on-death explosion damage of Tier 2 enemies during the {name} Mutator."
            );

            _tier3ExplosionRadius = config.Bind(
            GetSection(name),
            "Tier 3 Enemy Explosion Radius",
            3f,
            $"The on-death explosion radius of Tier 3 enemies during the {name} Mutator."
            );

            _tier3ExplosionDamage = config.BindPositive(
            GetSection(name),
            "Tier 3 Enemy Explosion Damage",
            200,
            $"The on-death explosion damage of Tier 3 enemies during the {name} Mutator."
            );
        }

        /// <inheritdoc cref="AbstractMutatorSettings.CreateMetadata"/>
        /// <returns>A dictionary holding <c>tier-1-enemy-explosion-radius</c>, <c>tier-1-enemy-explosion-damage</c>, <c>tier-2-enemy-explosion-radius</c>, <c>tier-2-enemy-explosion-damage</c>, <c>tier-3-enemy-explosion-radius</c>, and <c>tier-3-enemy-explosion-damage</c></returns>
        protected override IDictionary<string, object>? CreateMetadata()
        {
            return new Dictionary<string, object>
            {
                { Tier1Radius, Tier1ExplosionRadius },
                { Tier1Damage, Tier1ExplosionDamage },
                { Tier2Radius, Tier2ExplosionRadius },
                { Tier2Damage, Tier2ExplosionDamage },
                { Tier3Radius, Tier3ExplosionRadius },
                { Tier3Damage, Tier3ExplosionDamage }
            };
        }
    }
}
