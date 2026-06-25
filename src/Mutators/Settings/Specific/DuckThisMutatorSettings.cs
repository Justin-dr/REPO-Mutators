using BepInEx.Configuration;

namespace Mutators.Settings.Specific
{
    /// <summary>
    /// Settings for the Duck This mutator.
    /// </summary>
    public class DuckThisMutatorSettings : GenericMutatorSettings
    {
        /// <summary>
        /// Metadata key for the duck aggro cooldown.
        /// </summary>
        public const string AggroCooldownKey = "aggro-cooldown";

        private readonly ConfigEntry<float> _duckAggroCooldown;
        /// <summary>
        /// The cooldown between duck aggro while the Duck This Mutator is active.
        /// </summary>
        public float AggroCooldown => GetClampedRuntimeOverride(AggroCooldownKey, _duckAggroCooldown);

        internal DuckThisMutatorSettings(string name, string description, ConfigFile config) : base(MyPluginInfo.PLUGIN_GUID, name, description, config)
        {
            _duckAggroCooldown = config.Bind(
                GetSection(name),
                "Duck aggro cooldown",
                50f,
                new ConfigDescription(
                    $"The cooldown between duck aggro while the {Mutators.Mutators.DuckThisName} Mutator is active.",
                    new AcceptableValueRange<float>(0, 300)
                )
            );
        }
    }
}
