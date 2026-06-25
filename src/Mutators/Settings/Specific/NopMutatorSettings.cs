using BepInEx.Configuration;
using Mutators.Extensions;

namespace Mutators.Settings.Specific
{
    /// <summary>
    /// Settings for the <see cref="Mutators.NopMutator"/>.
    /// </summary>
    public sealed class NopMutatorSettings : AbstractMutatorSettings
    {
        private readonly ConfigEntry<int> _weight;
        private readonly ConfigEntry<int> _minimumLevel;
        private readonly ConfigEntry<int> _maximumLevel;

        /// <summary>
        /// This mutator serves as a default and is not weighted.
        /// This property should be considered a percentage chance.
        /// </summary>
        public override int Weight => _weight.Value;

        /// <summary>
        /// <inheritdoc cref="AbstractMutatorSettings.MinimumLevel"/>
        /// </summary>
        public override int MinimumLevel => _minimumLevel.Value;

        /// <summary>
        /// <inheritdoc cref="AbstractMutatorSettings.MinimumLevel"/>
        /// </summary>
        public override int MaximumLevel => _maximumLevel.Value;

        internal NopMutatorSettings(ConfigFile config) : base(MyPluginInfo.PLUGIN_GUID, Mutators.Mutators.NopMutatorName, Mutators.Mutators.NopMutatorDescription)
        {
            _weight = config.Bind(
            "No Mutator",
            "Chance (%)",
            50,
            new ConfigDescription(
                "Percentage chance for no mutator to be active. Unlike other mutators, this one is not weighted",
                new AcceptableValueRange<int>(0, 100)
            ));

            _minimumLevel = config.BindPositive(
            "No Mutator",
            "Minimum level",
            0,
            "The minimum level on which no mutator can show up"
            );

            _maximumLevel = config.BindPositive(
            "No Mutator",
            "Maximum level",
            1000,
            "The maximum level on which no mutator can show up (0 = no upper bound)"
            );
        }
    }
}
