using BepInEx.Configuration;

namespace Mutators.Settings.Specific
{
    public class HandleWithCareMutatorSettings : GenericMutatorSettings
    {
        private readonly ConfigEntry<float> _valueMultiplier;
        private readonly ConfigEntry<bool> _instaDestroySurplus;
        private readonly ConfigEntry<bool> _multiplySurplus;
        public float ValueMultiplier => _valueMultiplier.Value;
        public bool InstantlyDestroySurplus => _instaDestroySurplus.Value;
        public bool MultiplySurplusValue => _multiplySurplus.Value;

        internal HandleWithCareMutatorSettings(string name, ConfigFile config) : base(name, config)
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
