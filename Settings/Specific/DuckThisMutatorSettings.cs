using BepInEx.Configuration;

namespace Mutators.Settings.Specific
{
    public class DuckThisMutatorSettings : GenericMutatorSettings
    {
        private readonly ConfigEntry<float> _duckAggroCooldown;
        public float AggroCooldown => _duckAggroCooldown.Value;
        internal DuckThisMutatorSettings(string name, ConfigFile config) : base(name, config)
        {
            _duckAggroCooldown = config.Bind(
                GetSection(name),
                "Duck aggro cooldown",
                120f,
                $"The cooldown between duck aggro while the {Mutators.Mutators.DuckThis} Mutator is active."
            );
        }
    }
}
