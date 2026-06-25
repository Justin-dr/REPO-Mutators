using BepInEx.Configuration;
using Mutators.Extensions;

namespace Mutators.Settings.Specific
{
    /// <summary>
    /// Settings for the There Can Only Be One mutator.
    /// </summary>
    public class ThereCanOnlyBeOneMutatorSettings : EnemyDisablingMutatorSettings
    {
        /// <summary>
        /// Metadata key for the level at which grouped enemy spawns are allowed.
        /// </summary>
        public const string GroupSpawnsThresholdKey = "group-spawns-threshold";

        private readonly ConfigEntry<int> _groupSpawnsThreshold;

        /// <summary>
        /// The minimum level from which grouped enemy spawns can be used.
        /// </summary>
        public int GroupSpawnsThreshold => GetClampedRuntimeOverride(GroupSpawnsThresholdKey, _groupSpawnsThreshold);

        internal ThereCanOnlyBeOneMutatorSettings(string name, string description, ConfigFile config) : base(MyPluginInfo.PLUGIN_GUID, name, description, config)
        {
            _groupSpawnsThreshold = config.BindPositive(
            GetSection(name),
            "Group spawn minimum level",
            8,
            $"The minimum level from which enemy groups (e.g. 3 mentalists or 10 gnomes) can be spawned by the {name} Mutator."
            );
        }
    }
}
