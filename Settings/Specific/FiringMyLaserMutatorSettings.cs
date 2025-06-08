using BepInEx.Configuration;

namespace Mutators.Settings.Specific
{
    public class FiringMyLaserMutatorSettings : GenericMutatorSettings
    {
        private readonly ConfigEntry<int> _laserActionCooldown;
        private readonly ConfigEntry<int> _laserActionEnemyDamage;
        private readonly ConfigEntry<bool> _laserActionEnabled;
        public int LaserActionCooldown => _laserActionCooldown.Value;
        public int LaserActionEnemyDamage => _laserActionEnemyDamage.Value;
        public bool LaserActionEnabled => _laserActionEnabled.Value;

        internal FiringMyLaserMutatorSettings(string name, string description, ConfigFile config) : base(name, description, config)
        {
            _laserActionEnabled = config.Bind(
            GetSection(name),
            "Allow manual laser action",
            true,
            $"If true, players can manually use their laser action while the cooldown is over. Otherwise, only use the laser when getting hit"
            );

            _laserActionCooldown = config.Bind(
            GetSection(name),
            "Laser action cooldown",
            60,
            new ConfigDescription(
                $"The amount of seconds before the laser special action can be used again.",
                new AcceptableValueRange<int>(5, 10000))
            );

            _laserActionEnemyDamage = config.Bind(
            GetSection(name),
            "Laser action enemy damage",
            30,
            new ConfigDescription(
                $"The amount of damage the laser special action deals to enemies per tick.",
                new AcceptableValueRange<int>(10, 200))
            );
        }
    }
}
