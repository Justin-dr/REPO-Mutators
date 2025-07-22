using System;
using System.Collections.Generic;
using System.Linq;
using Mutators.Managers;
using Mutators.Settings;

namespace Mutators.Extensions;

internal static class LevelRemovingMutatorSettingsExtensions
{
    private const string TRUCK_LEVEL_NAME = "Level - Lobby";
    private static readonly ISet<string> vanillaLevelNames = new HashSet<string>()
    {
        { "Level - Artic" },
        { "Level - Manor" },
        { "Level - Wizard" },
        { "Level - Museum" }
    };
    
    internal static void RemoveLevels(this ILevelRemovingMutatorSettings settings, bool lobbyMenu = false)
    {
        RunManager runManager = RunManager.instance;
        if (!lobbyMenu && runManager.levelCurrent.name != TRUCK_LEVEL_NAME) return;
            
        if (!settings.AllowCustomLevels)
        {
            LevelManager.Instance.RemoveLevels(vanillaLevelNames);
        }

        if (settings.ExcludedLevels.Count > 0)
        {
            ISet<string> excludedSet = new HashSet<string>(
                settings.ExcludedLevels.Select(level => level.StartsWith("level - ", StringComparison.OrdinalIgnoreCase) ? level.ToLowerInvariant() : ("level - " + level).ToLowerInvariant())
            );
                
            LevelManager.Instance.RemoveLevels(excludedSet);
        }

        if (runManager.levels.Count == 1)
        {
            runManager.previousRunLevel = null!;
        }
        else if (runManager.levels.Count == 0)
        {
            runManager.previousRunLevel = null!;
            RepoMutators.Logger.LogError("Attempted to start a run with 0 available levels, please revisit your mod settings!");
            RepoMutators.Logger.LogError("There must be at least one level available to choose from!");
        }
    }
}