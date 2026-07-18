using System.Collections.Generic;
using Mutators.Extensions;
using Mutators.Managers;
using Mutators.Mutators;
using Mutators.Settings;

namespace Mutators.Utility
{
    internal static class LevelRemovalUtils
    {
        internal static void RemoveLevels(bool lobbyMenu = false)
        {
            IMutator currentMutator = MutatorManager.Instance.CurrentMutator;
            if (currentMutator is IMultiMutator multiMutator)
            {
                foreach (KeyValuePair<IMutator, IDictionary<string, object>> subMutator in multiMutator.SubMutators)
                {
                    subMutator.Key.Settings.ClearRuntimeOverrides();
                    subMutator.Key.Settings.ApplyRuntimeOverrides(subMutator.Value);

                    if (subMutator.Key.Settings is ILevelRemovingMutatorSettings levelRemovingMutatorSettings)
                    {
                        levelRemovingMutatorSettings.RemoveLevels(lobbyMenu);
                    }
                }

                return;
            }
            
            if (currentMutator.Settings is ILevelRemovingMutatorSettings settings)
            {
                settings.RemoveLevels(lobbyMenu);
            }
        }
    }
}