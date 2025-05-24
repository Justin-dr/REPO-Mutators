using BepInEx.Configuration;

namespace Mutators.Settings.Specific
{
    public class NopMutatorSettings : AbstractMutatorSettings
    {
        private readonly ConfigEntry<uint> _weight;
        private readonly ConfigEntry<uint> _minimumLevel;
        private readonly ConfigEntry<uint> _maximumLevel;

        public override string MutatorName => Mutators.Mutators.NopMutatorName;

        public override string MutatorDescription => Mutators.Mutators.NopMutatorDescription;

        public override uint Weight => _weight.Value;

        public override uint MinimumLevel => _minimumLevel.Value;

        public override uint MaximumLevel => _maximumLevel.Value;

        internal NopMutatorSettings(ConfigFile config)
        {
            _weight = config.Bind<uint>(
            "No Mutator",
            "Weight",
            (uint)((Mutators.Mutators.All().Length - 1) * 100L),
            "Weighted chance for no mutator to be active."
            );

            _minimumLevel = config.Bind<uint>(
            "No Mutator",
            "Minimum level",
            0,
            "The minimum level on which no mutator can show up"
            );

            _maximumLevel = config.Bind<uint>(
            "No Mutator",
            "Maximum level",
            1000,
            "The maximum level on which no mutator can show up (0 = no upper bound)"
            );
        }
    }
}
