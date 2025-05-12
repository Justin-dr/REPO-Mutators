using BepInEx.Configuration;

namespace Mutators.Settings.Specific
{
    public class ApolloElevenMutatorSettings : GenericMutatorSettings
    {
        private readonly ConfigEntry<bool> _applyToEnemies;
        public bool ApplyToEnemies => _applyToEnemies.Value;
        internal ApolloElevenMutatorSettings(string name, ConfigFile config) : base(name, config)
        {
            _applyToEnemies = config.Bind(
                GetSection(name),
                "Apply to monsters",
                false,
                $"If true, Zero-Gravity will also be applied to monsters while the {name} Mutator is active."
            );
        }
    }
}
