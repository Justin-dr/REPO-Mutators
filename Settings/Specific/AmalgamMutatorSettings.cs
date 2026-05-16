using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mutators.Settings.Specific
{
    public class AmalgamMutatorSettings : GenericMutatorSettings, ILevelRemovingMutatorSettings
    {
        public const string ExcludedLevelsKey = "excludedLevels";

        private readonly ConfigEntry<string> _excludedLevels;
        private IList<string> _excludedLevelsList;

        public bool AllowCustomLevels => true;

        public IList<string> ExcludedLevels => ExcludedLevelsWithRequiredLevels(
            GetRuntimeOverrideList(ExcludedLevelsKey, _excludedLevelsList)
        );

        internal AmalgamMutatorSettings(string name, string description, ConfigFile config) : base(name, description, config)
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

        private static IList<string> ExcludedLevelsWithRequiredLevels(IList<string> excluded)
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
