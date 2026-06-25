using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;

namespace Mutators.Settings.Specific
{
    /// <summary>
    /// Settings for the Amalgam mutator.
    /// </summary>
    public class AmalgamMutatorSettings : GenericMutatorSettings, ILevelRemovingMutatorSettings
    {
        /// <summary>
        /// Metadata key for the levels excluded from the Amalgam mutator.
        /// </summary>
        public const string ExcludedLevelsKey = "excluded-levels";

        private readonly ConfigEntry<string> _excludedLevels;
        private IList<string> _excludedLevelsList;

        /// <summary>
        /// Whether the Amalgam mutator should allow custom levels.
        /// </summary>
        public bool AllowCustomLevels => true;

        /// <summary>
        /// List of level names that should be excluded from the Amalgam mutator.
        /// <remarks>
        /// Backrooms and PeachCastle are always excluded.
        /// </remarks>
        /// </summary>
        public IList<string> ExcludedLevels => GetExcludedLevelsWithDefault(
            GetRuntimeOverrideList(ExcludedLevelsKey, _excludedLevelsList)
        );

        internal AmalgamMutatorSettings(string name, string description, ConfigFile config) : base(MyPluginInfo.PLUGIN_GUID, name, description, config)
        {
            _excludedLevels = config.Bind<string>(
                GetSection(name),
                "Excluded levels",
                "",
                $"Levels that should be excluded from the {name} Mutator. These can neither be picked as a base, nor will their rooms be used in the level generation"
            );

            _excludedLevelsList = ExcludedLevelsAsList();
            _excludedLevels.SettingChanged += SettingChanged;
        }

        private void SettingChanged(object sender, EventArgs e)
        {
            _excludedLevelsList = ExcludedLevelsAsList();
        }

        private IList<string> ExcludedLevelsAsList()
        {
            return _excludedLevels.Value.Split(",").Select(value => value.Trim()).ToList();
        }

        private static IList<string> GetExcludedLevelsWithDefault(IList<string> excluded)
        {
            IList<string> levels = excluded.ToList();

            if (!levels.Contains("Backrooms"))
            {
                levels.Add("Backrooms");
            }
            
            if (!levels.Contains("PeachCastle"))
            {
                levels.Add("PeachCastle");
            }

            return levels;
        }
    }
}
