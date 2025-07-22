using BepInEx.Configuration;
using Mutators.Extensions;

namespace Mutators.Settings.Specific
{
    /// <summary>
    /// Settings for the One Shot, One Kill mutator.
    /// </summary>
    public class OneShotOneKillMutatorSettings : EnemyDisablingMutatorSettings
    {
        /// <summary>
        /// Metadata key for whether heads should instantly revive in the truck or extraction point.
        /// </summary>
        public const string InstaReviveInTruckOrExtractionKey = "revive-in-truck-or-extraction";

        /// <summary>
        /// Metadata key for the health used by instant revives.
        /// </summary>
        public const string InstaReviveHealthKey = "revive-health";

        private readonly ConfigEntry<bool> _instaReviveInTruckOrExtraction;
        private readonly ConfigEntry<int> _instaReviveHealth;

        /// <summary>
        /// Whether players are instantly revived when their head is brought to the truck or an active extraction point.
        /// </summary>
        public bool InstaReviveInTruckOrExtraction => GetRuntimeOverride(InstaReviveInTruckOrExtractionKey, _instaReviveInTruckOrExtraction.Value);

        /// <summary>
        /// The health players receive when instantly revived.
        /// </summary>
        public int InstaReviveHealth => GetClampedRuntimeOverride(InstaReviveHealthKey, _instaReviveHealth);

        internal OneShotOneKillMutatorSettings(string name, string description, ConfigFile config) : base(MyPluginInfo.PLUGIN_GUID, name, description, config, "Peeper")
        {
            _instaReviveInTruckOrExtraction = config.Bind(
            GetSection(name),
            "Instant revive in truck or extraction",
            false,
            $"If true, while the {name} Mutator is active, players are instantly revived with full head if their head is brought to the truck or an active extraction point."
            );

            _instaReviveHealth = config.BindPositive(
            GetSection(name),
            "Instant revive health",
            0,
            "The amount of health the player should be revived with when Instant revive is enabled. (0 = Revive with max health)"
            );
        }
    }
}
