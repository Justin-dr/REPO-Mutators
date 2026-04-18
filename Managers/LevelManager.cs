using System;
using System.Collections.Generic;
using System.Linq;

namespace Mutators.Managers;

public class LevelManager
{
    private static readonly IDictionary<string, Level> RemovedLevels = new Dictionary<string, Level>(StringComparer.OrdinalIgnoreCase);
    
    public static LevelManager Instance { get; private set; } = new();

    public void RemoveLevels(ICollection<string> levelsToRemove)
    {
        if (levelsToRemove.Count == 0) return;

        List<Level> levels = RunManager.instance.levels;
        ISet<string> removeSet = new HashSet<string>(levelsToRemove, StringComparer.OrdinalIgnoreCase);
        List<Level> removedLevels = levels.FindAll(level => removeSet.Contains(level.name));

        foreach (Level level in removedLevels)
        {
            RemovedLevels[level.name] = level;
        }

        levels.RemoveAll(level => removeSet.Contains(level.name));
    }

    public void RestoreLevels()
    {
        if (RemovedLevels.Count == 0) return;
        
        List<Level> levels = RunManager.instance.levels;
        ISet<string> existingNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (Level level in levels)
        {
            existingNames.Add(level.name);
        }

        levels.AddRange(RemovedLevels.Values.Where(level => existingNames.Add(level.name)));

        foreach (Level removedLevel in RemovedLevels.Values)
        {
            RepoMutators.Logger.LogInfo($"Restoring level {removedLevel.name}");
        }

        foreach (Level level in RunManager.instance.levels)
        {
            RepoMutators.Logger.LogInfo($"Level {level.name} is now active");
        }

        RemovedLevels.Clear();
    }
}
