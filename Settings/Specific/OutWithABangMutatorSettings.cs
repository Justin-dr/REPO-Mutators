using BepInEx.Configuration;
using System.Collections.Generic;

namespace Mutators.Settings.Specific
{
    public class OutWithABangMutatorSettings : GenericMutatorSettings
    {
        public const string Tier1Radius = "OutWithABang-Radius1";
        public const string Tier1Damage = "OutWithABang-Damage1";
        public const string Tier2Radius = "OutWithABang-Radius2";
        public const string Tier2Damage = "OutWithABang-Damage2";
        public const string Tier3Radius = "OutWithABang-Radius3";
        public const string Tier3Damage = "OutWithABang-Damage3";

        private readonly ConfigEntry<float> _tier1ExplosionRadius;
        private readonly ConfigEntry<int> _tier1ExplosionDamage;
        private readonly ConfigEntry<float> _tier2ExplosionRadius;
        private readonly ConfigEntry<int> _tier2ExplosionDamage;
        private readonly ConfigEntry<float> _tier3ExplosionRadius;
        private readonly ConfigEntry<int> _tier3ExplosionDamage;

        public float Tier1ExplosionRadius => _tier1ExplosionRadius.Value;
        public int Tier1ExplosionDamage => _tier1ExplosionDamage.Value;
        public float Tier2ExplosionRadius => _tier2ExplosionRadius.Value;
        public int Tier2ExplosionDamage => _tier2ExplosionDamage.Value;
        public float Tier3ExplosionRadius => _tier3ExplosionRadius.Value;
        public int Tier3ExplosionDamage => _tier3ExplosionDamage.Value;

        internal OutWithABangMutatorSettings(string name, string description, ConfigFile config) : base(name, description, config)
        {
            _tier1ExplosionRadius = config.Bind(
            GetSection(name),
            "Tier 1 Enemy Explosion Radius",
            1f,
            $"The on-death explosion radius of Tier 1 enemies during the {name} Mutator."
            );

            _tier1ExplosionDamage = config.Bind(
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

            _tier2ExplosionDamage = config.Bind(
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

            _tier3ExplosionDamage = config.Bind(
            GetSection(name),
            "Tier 3 Enemy Explosion Damage",
            200,
            $"The on-death explosion damage of Tier 3 enemies during the {name} Mutator."
            );
        }

        public override IDictionary<string, object>? AsMetadata()
        {
            IDictionary<string, object> metadata = new Dictionary<string, object>
            {
                { Tier1Radius, Tier1ExplosionRadius },
                { Tier1Damage, Tier1ExplosionDamage },
                { Tier2Radius, Tier2ExplosionRadius },
                { Tier2Damage, Tier2ExplosionDamage },
                { Tier3Radius, Tier3ExplosionRadius },
                { Tier3Damage, Tier3ExplosionDamage }
            };

            return metadata;
        }
    }
}
