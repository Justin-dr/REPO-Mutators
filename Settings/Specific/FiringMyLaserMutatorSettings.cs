using BepInEx.Configuration;

namespace Mutators.Settings.Specific
{
    public class FiringMyLaserMutatorSettings : GenericMutatorSettings
    {
        private readonly ConfigEntry<int> _laserActionCooldown;
        public int LaserActionCooldown => _laserActionCooldown.Value;

        internal FiringMyLaserMutatorSettings(string name, string description, ConfigFile config) : base(name, description, config)
        {
            _laserActionCooldown = config.Bind(
            GetSection(name),
            "Laser action cooldown",
            60,
            new ConfigDescription(
                $"The amount of seconds before the laser special action can be used again.",
                new AcceptableValueRange<int>(5, 10000))
            );
        }
    }
}
