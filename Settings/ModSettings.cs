using BepInEx.Configuration;

namespace Mutators.Settings
{
    internal class ModSettings
    {
        private readonly ConfigEntry<float> _mutatorDisplayY;
        public float MutatorDisplayY => _mutatorDisplayY.Value;
        internal ModSettings(ConfigFile config)
        {
            _mutatorDisplayY = config.Bind(
                    "Mutator Interface",
                    "Y position",
                    -75f,
                    "The Y position of the active Mutator overlay"
            );
        }
    }
}
