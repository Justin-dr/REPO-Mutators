using BepInEx.Configuration;

namespace Mutators.Settings.Specific
{
    public class LessIsMoreMutatorSettings : GenericMutatorSettings
    {
        private readonly ConfigEntry<float> _strongDivisionFactor;
        private readonly ConfigEntry<float> _weakDivisionFactor;
        private readonly ConfigEntry<float> _valueGainMultiplier;

        public float StrongDivisionFactor => _strongDivisionFactor.Value;
        public float WeakDivisionFactor => _weakDivisionFactor.Value;
        public float ValueGainMultiplier => _valueGainMultiplier.Value;

        internal LessIsMoreMutatorSettings(string name, string description, ConfigFile config) : base(name, description, config)
        {
            _strongDivisionFactor = config.Bind(
            GetSection(name),
            "Strong Durability Division Factor",
            1f,
            new ConfigDescription(
                $"The amount by which the value of strong durability valuables should be divided when the {name} Mutator is active. Acts as a lower bound.",
                new AcceptableValueRange<float>(1, 5)
                )
            );

            _weakDivisionFactor = config.Bind(
            GetSection(name),
            "Weak Durability Division Factor",
            2f,
            new ConfigDescription(
                $"The amount by which the value of weak durability valuables should be divided when the {name} Mutator is active. Acts as an upper bound.",
                new AcceptableValueRange<float>(1, 5)
                )
            );

            _valueGainMultiplier = config.Bind(
            GetSection(name),
            "Value Gain Multiplier",
            2f,
            new ConfigDescription(
                $"The amount by which the normal value gain should be multiplied when the {name} Mutator is active.",
                new AcceptableValueRange<float>(1, 10)
                )
            );
        }
    }
}
