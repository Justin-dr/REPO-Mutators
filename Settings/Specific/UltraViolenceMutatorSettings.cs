using BepInEx.Configuration;

namespace Mutators.Settings.Specific
{
    public class UltraViolenceMutatorSettings : EnemyDisablingMutatorSettings
    {
        private ConfigEntry<bool> _keepOnLight;
        public bool KeepOnLight => _keepOnLight.Value;

        internal UltraViolenceMutatorSettings(string name, string description, ConfigFile config) : base(name, description, config, [])
        {
            _keepOnLight = config.Bind(
            GetSection(name),
            "Keep on lights",
            false,
            $"Keep on the level lighting while the {name} Mutator is active."
            );
        }
    }
}
