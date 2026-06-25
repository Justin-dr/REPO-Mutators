using BepInEx.Configuration;

namespace Mutators.Settings.Specific
{
    /// <summary>
    /// Settings for the Handle With Care mutator.
    /// </summary>
    public class HandleWithCareMutatorSettings : GenericMutatorSettings
    {
        /// <summary>
        /// Metadata key for the valuable value multiplier.
        /// </summary>
        public const string ValueMultiplierKey = "value-multiplier";

        /// <summary>
        /// Metadata key for whether surplus value should be destroyed instantly.
        /// </summary>
        public const string InstantlyDestroySurplusKey = "instantly-destroy-surplus";

        /// <summary>
        /// Metadata key for whether surplus value should be multiplied.
        /// </summary>
        public const string MultiplySurplusValueKey = "multiply-surplus-value";

        private readonly ConfigEntry<float> _valueMultiplier;
        private readonly ConfigEntry<bool> _instaDestroySurplus;
        private readonly ConfigEntry<bool> _multiplySurplus;
        
        /// <summary>
        /// The amount by which the value of valuables should be multiplier when the Handle With Care Mutator is active.
        /// </summary>
        public float ValueMultiplier => GetClampedRuntimeOverride(ValueMultiplierKey, _valueMultiplier);
        
        /// <summary>
        /// Whether the surplus value should be destroyed instantly when taking damage.
        /// </summary>
        public bool InstantlyDestroySurplus => GetClampedRuntimeOverride(InstantlyDestroySurplusKey, _instaDestroySurplus);
        
        /// <summary>
        /// Whether the surplus value should be multiplied.
        /// </summary>
        public bool MultiplySurplusValue => GetClampedRuntimeOverride(MultiplySurplusValueKey, _multiplySurplus);

        internal HandleWithCareMutatorSettings(string name, string description, ConfigFile config) : base(MyPluginInfo.PLUGIN_GUID, name, description, config)
        {
            _valueMultiplier = config.Bind<float>(
            GetSection(name),
            "Value Multiplier",
            2,
            new ConfigDescription(
                $"The amount by which the value of valuables should be multiplier when {name} is active.",
                new AcceptableValueRange<float>(1f, 10f)
                )
            );

            _instaDestroySurplus = config.Bind(
            GetSection(name),
            "Instantly Destroy Surplus on Damage",
            false,
            $"If true, while the {name} Mutator is active, the surplus will instantly be destroyed when taking any damage, just like other valuables."
            );

            _multiplySurplus = config.Bind(
            GetSection(name),
            "Multiply Surplus Value",
            false,
            $"If true, while the {name} Mutator is active, the surplus value will be multiplied, just like other valuables."
            );
        }
    }
}
