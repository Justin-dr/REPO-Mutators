using Sirenix.Utilities;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Mutators.Settings
{
    public abstract class AbstractMutatorSettings
    {
        protected const string WeightConfigKey = "Weight";
        protected const string MinimumLevelConfigKey = "Minimum level";
        protected const string MaximumLevelConfigKey = "Maximum level";
        public abstract string MutatorName { get; }
        public abstract string MutatorDescription { get; }
        public abstract uint Weight { get; }
        public abstract uint MinimumLevel { get; }
        public abstract uint MaximumLevel { get; }

        public virtual bool IsEligibleForSelection()
        {
            int levelsCompleted = RunManager.instance.levelsCompleted;

            if (MaximumLevel > 0 && MinimumLevel > MaximumLevel)
            {
                RepoMutators.Logger.LogWarning($"{MutatorName} was configured with a minimum level larger than the maximum level!");
                RepoMutators.Logger.LogWarning($"This configuration is consider invalid, the level bounds will be ignored.");
                return true;
            }

            if (MaximumLevel == 0)
            {
                return levelsCompleted >= MinimumLevel;
            }

            return levelsCompleted >= MinimumLevel && levelsCompleted <= MaximumLevel;
        }

        public virtual IDictionary<string, object>? AsMetadata()
        {
            return null;
        }
    }
}
