using System.Collections.Generic;
using BepInEx.Configuration;
using Mutators.Mutators.Patches;

namespace Mutators.Settings.Specific
{
    /// <summary>
    /// Settings for the Less Is More mutator.
    /// </summary>
    public class LessIsMoreMutatorSettings : GenericMutatorSettings
    {
        /// <summary>
        /// Metadata key for the strong durability value division factor.
        /// </summary>
        public const string StrongDivisionFactorKey = "strong-division-factor";

        /// <summary>
        /// Metadata key for the weak durability value division factor.
        /// </summary>
        public const string WeakDivisionFactorKey = "weak-division-factor";

        private readonly ConfigEntry<float> _strongDivisionFactor;
        private readonly ConfigEntry<float> _weakDivisionFactor;
        private readonly ConfigEntry<float> _valueGainMultiplier;

        /// <summary>
        /// The amount by which the value of strong durability valuables should be divided.
        /// </summary>
        public float StrongDivisionFactor => GetClampedRuntimeOverride(StrongDivisionFactorKey, _strongDivisionFactor);

        /// <summary>
        /// The amount by which the value of weak durability valuables should be divided.
        /// </summary>
        public float WeakDivisionFactor => GetClampedRuntimeOverride(WeakDivisionFactorKey, _weakDivisionFactor);

        /// <summary>
        /// The amount by which normal value gain should be multiplied.
        /// </summary>
        public float ValueGainMultiplier => _valueGainMultiplier.Value;

        internal LessIsMoreMutatorSettings(string name, string description, ConfigFile config) : base(MyPluginInfo.PLUGIN_GUID, name, description, config)
        {
            _weakDivisionFactor = config.Bind(
            GetSection(name),
            "Weak Durability Division Factor",
            1f,
            new ConfigDescription(
                $"The amount by which the value of weak durability valuables should be divided when the {name} Mutator is active. Acts as an lower bound.",
                new AcceptableValueRange<float>(1, 5))
            );

            _strongDivisionFactor = config.Bind(
            GetSection(name),
            "Strong Durability Division Factor",
            2f,
            new ConfigDescription(
                $"The amount by which the value of strong durability valuables should be divided when the {name} Mutator is active. Acts as a upper bound.",
                new AcceptableValueRange<float>(1, 5))
            );

            _valueGainMultiplier = config.Bind(
            GetSection(name),
            "Value Gain Multiplier",
            2f,
            new ConfigDescription(
                $"The amount by which the normal value gain should be multiplied when the {name} Mutator is active.",
                new AcceptableValueRange<float>(1, 10))
            );
        }

        /// <inheritdoc cref="AbstractMutatorSettings.CreateMetadata"/>
        /// <returns>A dictionary holding <c>value-gain-multiplier</c></returns>
        protected override IDictionary<string, object>? CreateMetadata()
        {
            return new Dictionary<string, object>
            {
                { LessIsMorePatch.ValueGainMultiplier, ValueGainMultiplier }
            };
        }
    }
}
