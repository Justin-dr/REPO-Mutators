using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mutators.Settings.Specific
{
    public class AmalgamMutatorSettings : GenericMutatorSettings, ILevelRemovingMutatorSettings
    {
        private readonly ConfigEntry<string> _excludedLevels;

        public bool AllowCustomLevels => true;

        public IList<string> ExcludedLevels { get; private set; }
        internal AmalgamMutatorSettings(string name, string description, ConfigFile config) : base(name, description, config)
        {
            _excludedLevels = config.Bind<string>(
                GetSection(name),
                "Excluded levels",
                "",
                $"Levels that should be excluded from the {name} Mutator. These can neither be picked as a base, nor will their rooms be used in the level generation"
            );

            ExcludedLevels = ExcludedLevelsAsList();
            _excludedLevels.SettingChanged += SettingChanged;
        }

        private void SettingChanged(object sender, EventArgs e)
        {
            ExcludedLevels = ExcludedLevelsAsList();
        }

        private IList<string> ExcludedLevelsAsList()
        {
            IList<string> excluded = _excludedLevels.Value.Split(",").Select(value => value.Trim()).ToList();

            if (!excluded.Contains("Backrooms"))
            {
                excluded.Add("Backrooms");
            }

            return excluded;
        }
    }
}
