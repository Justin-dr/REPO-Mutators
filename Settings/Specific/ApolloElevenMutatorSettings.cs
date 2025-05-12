using BepInEx.Configuration;
using System;
using UnityEngine;

namespace Mutators.Settings.Specific
{
    public class ApolloElevenMutatorSettings : GenericMutatorSettings
    {
        private readonly ConfigEntry<bool> _applyToEnemies;
        private readonly ConfigEntry<string> _downwardsKey;
        public bool ApplyToEnemies => _applyToEnemies.Value;
        public KeyCode DownwardsKey { get; private set; }
        internal ApolloElevenMutatorSettings(string name, ConfigFile config) : base(name, config)
        {
            _applyToEnemies = config.Bind(
                GetSection(name),
                "Apply to monsters",
                false,
                $"If true, Zero-Gravity will also be applied to monsters while the {name} Mutator is active."
            );

            _downwardsKey = config.Bind(
                GetSection(name),
                "Downwards momentum keybind",
                "LeftControl",
                $"(Client sided) If bound, this key can be used to control yourself downwards while the {name} Mutator is active"
            );

            CacheKey();
        }

        internal void CacheKey()
        {
            if (Enum.TryParse(typeof(KeyCode), _downwardsKey.Value, out object result))
            {
                DownwardsKey = (KeyCode) result;
                return;
            }
            DownwardsKey = KeyCode.None;
        }
    }
}
