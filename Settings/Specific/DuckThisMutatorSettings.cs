using BepInEx.Configuration;

namespace Mutators.Settings.Specific
{
    public class DuckThisMutatorSettings : GenericMutatorSettings
    {
        private readonly ConfigEntry<float> _duckAggroCooldown;
        public float AggroCooldown => _duckAggroCooldown.Value;
        internal DuckThisMutatorSettings(string name, string description, ConfigFile config) : base(name, description, config)
        {
            _duckAggroCooldown = config.Bind(
                GetSection(name),
                "Duck aggro cooldown",
                50f,
                $"The cooldown between duck aggro while the {Mutators.Mutators.DuckThisName} Mutator is active."
            );
        }
    }
}
