using System;
using BepInEx.Configuration;
using UnityEngine;

namespace Mutators.Settings.Specific
{
    /// <summary>
    /// Settings for the Apollo-Eleven mutator.
    /// </summary>
    public class ApolloElevenMutatorSettings : GenericMutatorSettings
    {
        /// <summary>
        /// Metadata key for whether the Apollo-Eleven mutator should apply to enemies.
        /// </summary>
        public const string ApplyToEnemiesKey = "apply-to-enemies";

        /// <summary>
        /// Metadata key for whether the Apollo-Eleven mutator should apply in the cart.
        /// </summary>
        public const string ApplyInCartKey = "apply-in-cart";

        private readonly ConfigEntry<bool> _applyToEnemies;
        private readonly ConfigEntry<bool> _applyInCart;
        private readonly ConfigEntry<string> _downwardsKey;
        
        /// <summary>
        /// Whether the Apollo-Eleven mutator should apply to monsters.
        /// </summary>
        public bool ApplyToEnemies => GetRuntimeOverride(ApplyToEnemiesKey, _applyToEnemies.Value);
        /// <summary>
        /// Whether the Apollo-Eleven mutator should apply to valuables in the cart and extraction points.
        /// </summary>
        public bool ApplyInCart => GetRuntimeOverride(ApplyInCartKey, _applyInCart.Value);
        /// <summary>
        /// The keybind used to force yourself downwards while the Apollo-Eleven mutator is active.
        /// </summary>
        public KeyCode DownwardsKey { get; private set; }
        internal ApolloElevenMutatorSettings(string name, string description, ConfigFile config) : base(MyPluginInfo.PLUGIN_GUID, name, description, config)
        {
            _applyToEnemies = config.Bind(
                GetSection(name),
                "Apply to monsters",
                false,
                $"If true, Zero-Gravity will also be applied to monsters while the {name} Mutator is active."
            );

            _applyInCart = config.Bind(
                GetSection(name),
                "Apply in cart/extraction",
                true,
                $"If true, Zero-Gravity will also apply to valuables in the cart and extraction points."
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
