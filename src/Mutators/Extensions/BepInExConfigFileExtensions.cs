using BepInEx.Configuration;
using Mutators.Utility.Config;

namespace Mutators.Extensions
{
    internal static class BepInExConfigFileExtensions
    {
        public static ConfigEntry<int> BindPositive(this ConfigFile configFile, string section, string key, int defaultValue, string description)
        {
            return configFile.Bind(
                section, key, defaultValue, new ConfigDescription(
                    description,
                    new PositiveIntValue()
                ));
        }
    }
}