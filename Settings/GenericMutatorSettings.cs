using BepInEx.Configuration;

namespace Mutators.Settings
{
    public class GenericMutatorSettings : AbstractMutatorSettings
    {
        private readonly ConfigEntry<uint> _weight;
        private readonly ConfigEntry<uint> _minimumLevel;
        private readonly ConfigEntry<uint> _maximumLevel;

        public override string MutatorName { get; }
        public override string MutatorDescription { get; }
        public override uint Weight => _weight.Value;
        public override uint MinimumLevel => _minimumLevel.Value;
        public override uint MaximumLevel => _maximumLevel.Value;

        public GenericMutatorSettings(string name, string description, ConfigFile config)
        {
            MutatorName = name;
            MutatorDescription = description;

            _weight = config.Bind<uint>(
                GetSection(name),
                WeightConfigKey,
                100,
                $"Weighted chance for the {name} Mutator to be active."
            );

            _minimumLevel = config.Bind<uint>(
                GetSection(name),
                MinimumLevelConfigKey,
                0,
                $"The minimum level on which the {name} Mutator can show up"
            );

            _maximumLevel = config.Bind<uint>(
                GetSection(name),
                MaximumLevelConfigKey,
                1000,
                $"The maximum level on which the {name} mutator can show up (0 = no upper bound)"
            );
        }

        protected static string GetSection(string name)
        {
            return $"{name} Mutator";
        }
    }
}
