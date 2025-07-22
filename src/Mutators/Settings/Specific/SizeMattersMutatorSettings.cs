using System.Collections.Generic;
using BepInEx.Configuration;

namespace Mutators.Settings.Specific
{
    /// <summary>
    /// Settings for the Size Matters mutator.
    /// </summary>
    public class SizeMattersMutatorSettings : GenericMutatorSettings
    {
        /// <summary>
        /// Metadata key for whether valuables should be scaled.
        /// </summary>
        public const string ScaleValuablesKey = "scale-valuables";

        /// <summary>
        /// Metadata key for whether enemies should be scaled.
        /// </summary>
        public const string ScaleEnemiesKey = "scale-enemies";

        /// <summary>
        /// Metadata key for whether carts should be scaled.
        /// </summary>
        public const string ScaleCartsKey = "scale-carts";
        
        private readonly ConfigEntry<bool> _scaleValuables;
        private readonly ConfigEntry<bool> _scaleEnemies;
        private readonly ConfigEntry<bool> _scaleCart;
        
        /// <summary>
        /// Whether valuables should be scaled while the Size Matters mutator is active.
        /// </summary>
        public bool ScaleValuables => GetRuntimeOverride(ScaleValuablesKey, _scaleValuables.Value);

        /// <summary>
        /// Whether enemies should be scaled while the Size Matters mutator is active.
        /// </summary>
        public bool ScaleEnemies => GetRuntimeOverride(ScaleEnemiesKey, _scaleEnemies.Value);

        /// <summary>
        /// Whether carts should be scaled while the Size Matters mutator is active.
        /// </summary>
        public bool ScaleCarts => GetRuntimeOverride(ScaleCartsKey, _scaleCart.Value);

        internal SizeMattersMutatorSettings(string name, string description, ConfigFile config) : base(MyPluginInfo.PLUGIN_GUID, name, description, config)
        {
            _scaleValuables = config.Bind(
                GetSection(name),
                "Scale valuables",
                false,
                "If true, valuables will also be scaled."
            );

            _scaleEnemies = config.Bind(
                GetSection(name),
                "Scale enemies",
                false,
                "If true, enemies will also be scaled."
            );
            
            _scaleCart = config.Bind(
                GetSection(name),
                "Scale carts",
                false,
                "If true, carts will also be scaled."
            );
        }

        /// <inheritdoc cref="AbstractMutatorSettings.CreateMetadata"/>
        /// <returns>A dictionary holding <c>scale-valuables</c>, <c>scale-enemies</c>, and <c>scale-carts</c></returns>
        protected override IDictionary<string, object> CreateMetadata()
        {
            return new Dictionary<string, object>
            {
                { ScaleValuablesKey, ScaleValuables },
                { ScaleEnemiesKey, ScaleEnemies },
                { ScaleCartsKey, ScaleCarts }
            };
        }
    }
}
