using BepInEx.Configuration;
using System;

namespace Mutators.Settings.Specific
{
    public class OneShotOneKillMutatorSettings : EnemyDisablingMutatorSettings
    {
        public const string InstaReviveInTruckOrExtractionKey = "instaReviveInTruckOrExtraction";
        public const string InstaReviveHealthKey = "instaReviveHealth";

        private readonly ConfigEntry<bool> _instaReviveInTruckOrExtraction;
        private readonly ConfigEntry<uint> _instaReviveHealth;
        public bool InstaReviveInTruckOrExtraction => GetRuntimeOverride(InstaReviveInTruckOrExtractionKey, _instaReviveInTruckOrExtraction.Value);
        public int InstaReviveHealth => GetRuntimeOverride(InstaReviveHealthKey, (int)Math.Clamp(_instaReviveHealth.Value, 0, int.MaxValue));

        internal OneShotOneKillMutatorSettings(string name, string description, ConfigFile config) : base(name, description, config, "Peeper")
        {
            _instaReviveInTruckOrExtraction = config.Bind(
            GetSection(name),
            "Instant revive in truck or extraction",
            false,
            $"If true, while the {name} Mutator is active, players are instantly revived with full head if their head is brought to the truck or an active extraction point."
            );

            _instaReviveHealth = config.Bind<uint>(
            GetSection(name),
            "Instant revive health",
            0,
            $"The amount of health the player should be revived with when Instant revive is enabled. (0 = Revive with max health)"
            );
        }
    }
}
