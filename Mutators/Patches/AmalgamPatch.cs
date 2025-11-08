using HarmonyLib;
using Mutators.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mutators.Mutators.Patches
{
    internal class AmalgamPatch
    {
        private static bool initDone = false;
        private static Level actualLevel = null!;
        private static readonly IDictionary<GameObject, Level> roomParentLevelMap = new Dictionary<GameObject, Level>();

        [HarmonyPrefix]
        [HarmonyPatch(typeof(LevelGenerator))]
        [HarmonyPatch(nameof(LevelGenerator.SpawnModule))]
        static void LevelGeneratorSpawnModulePrefix(LevelGenerator __instance)
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;
            if (!initDone)
            {
                actualLevel = __instance.Level;
                __instance.Level = new Level();

                actualLevel.ModulesNormal1.ForEach(module => roomParentLevelMap[module.Prefab] = actualLevel);
                actualLevel.ModulesNormal2.ForEach(module => roomParentLevelMap[module.Prefab] = actualLevel);
                actualLevel.ModulesNormal3.ForEach(module => roomParentLevelMap[module.Prefab] = actualLevel);

                actualLevel.ModulesPassage1.ForEach(module => roomParentLevelMap[module.Prefab] = actualLevel);
                actualLevel.ModulesPassage2.ForEach(module => roomParentLevelMap[module.Prefab] = actualLevel);
                actualLevel.ModulesPassage3.ForEach(module => roomParentLevelMap[module.Prefab] = actualLevel);

                actualLevel.ModulesDeadEnd1.ForEach(module => roomParentLevelMap[module.Prefab] = actualLevel);
                actualLevel.ModulesDeadEnd2.ForEach(module => roomParentLevelMap[module.Prefab] = actualLevel);
                actualLevel.ModulesDeadEnd3.ForEach(module => roomParentLevelMap[module.Prefab] = actualLevel);

                actualLevel.ModulesExtraction1.ForEach(module => roomParentLevelMap[module.Prefab] = actualLevel);
                actualLevel.ModulesExtraction2.ForEach(module => roomParentLevelMap[module.Prefab] = actualLevel);
                actualLevel.ModulesExtraction3.ForEach(module => roomParentLevelMap[module.Prefab] = actualLevel);

                RepoMutators.Logger.LogInfo("[Amalgam] Building level from the following available levels:");
                foreach (Level level in GetAllEligibleLevels())
                {
                    RepoMutators.Logger.LogInfo($"[Amalgam] {level.name}");
                    AddNormalModules(__instance, level);
                    AddPassageModules(__instance, level);
                    AddDeadEndModules(__instance, level);
                    AddExtractionModules(__instance, level);
                }

                initDone = true;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EnvironmentDirector))]
        [HarmonyPatch(nameof(EnvironmentDirector.Setup))]
        static void EnvironmentDirectorSetupPrefix()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer() && actualLevel != null)
            {
                LevelGenerator.Instance.Level = actualLevel;
                roomParentLevelMap.Clear();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(LevelGenerator))]
        [HarmonyPatch(nameof(LevelGenerator.PickModule))]
        static void LevelGeneratorPickModulePrefix(LevelGenerator __instance, ref GameObject __result)
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

            if (roomParentLevelMap.TryGetValue(__result, out Level level))
            {
                __instance.Level.ResourcePath = level.ResourcePath;
            }
            else
            {
                RepoMutators.Logger.LogError($"Failed to determine to which level module {__result.name} belongs!");
            }
        }

        static void AddNormalModules(LevelGenerator levelGenerator, Level level)
        {
            levelGenerator.ModulesNormalShuffled_1.AddRange(level.ModulesNormal1);
            levelGenerator.ModulesNormalShuffled_2.AddRange(level.ModulesNormal2);
            levelGenerator.ModulesNormalShuffled_3.AddRange(level.ModulesNormal3);

            levelGenerator.ModulesNormalShuffled_1.Shuffle();
            levelGenerator.ModulesNormalShuffled_2.Shuffle();
            levelGenerator.ModulesNormalShuffled_3.Shuffle();

            level.ModulesNormal1.ForEach(module => roomParentLevelMap[module.Prefab] = level);
            level.ModulesNormal2.ForEach(module => roomParentLevelMap[module.Prefab] = level);
            level.ModulesNormal3.ForEach(module => roomParentLevelMap[module.Prefab] = level);
        }

        static void AddPassageModules(LevelGenerator levelGenerator, Level level)
        {
            levelGenerator.ModulesPassageShuffled_1.AddRange(level.ModulesPassage1);
            levelGenerator.ModulesPassageShuffled_2.AddRange(level.ModulesPassage2);
            levelGenerator.ModulesPassageShuffled_3.AddRange(level.ModulesPassage3);

            levelGenerator.ModulesPassageShuffled_1.Shuffle();
            levelGenerator.ModulesPassageShuffled_2.Shuffle();
            levelGenerator.ModulesPassageShuffled_3.Shuffle();

            level.ModulesPassage1.ForEach(module => roomParentLevelMap[module.Prefab] = level);
            level.ModulesPassage2.ForEach(module => roomParentLevelMap[module.Prefab] = level);
            level.ModulesPassage3.ForEach(module => roomParentLevelMap[module.Prefab] = level);
        }

        static void AddDeadEndModules(LevelGenerator levelGenerator, Level level)
        {
            levelGenerator.ModulesDeadEndShuffled_1.AddRange(level.ModulesDeadEnd1);
            levelGenerator.ModulesDeadEndShuffled_2.AddRange(level.ModulesDeadEnd2);
            levelGenerator.ModulesDeadEndShuffled_3.AddRange(level.ModulesDeadEnd3);

            levelGenerator.ModulesDeadEndShuffled_1.Shuffle();
            levelGenerator.ModulesDeadEndShuffled_2.Shuffle();
            levelGenerator.ModulesDeadEndShuffled_3.Shuffle();

            level.ModulesDeadEnd1.ForEach(module => roomParentLevelMap[module.Prefab] = level);
            level.ModulesDeadEnd2.ForEach(module => roomParentLevelMap[module.Prefab] = level);
            level.ModulesDeadEnd3.ForEach(module => roomParentLevelMap[module.Prefab] = level);
        }

        static void AddExtractionModules(LevelGenerator levelGenerator, Level level)
        {
            levelGenerator.ModulesExtractionShuffled_1.AddRange(level.ModulesExtraction1);
            levelGenerator.ModulesExtractionShuffled_2.AddRange(level.ModulesExtraction2);
            levelGenerator.ModulesExtractionShuffled_3.AddRange(level.ModulesExtraction3);

            levelGenerator.ModulesExtractionShuffled_1.Shuffle();
            levelGenerator.ModulesExtractionShuffled_2.Shuffle();
            levelGenerator.ModulesExtractionShuffled_3.Shuffle();

            level.ModulesExtraction1.ForEach(module => roomParentLevelMap[module.Prefab] = level);
            level.ModulesExtraction2.ForEach(module => roomParentLevelMap[module.Prefab] = level);
            level.ModulesExtraction3.ForEach(module => roomParentLevelMap[module.Prefab] = level);
        }

        private static IList<Level> GetAllEligibleLevels()
        {
            ISet<string> excludedSet = new HashSet<string>(
                MutatorSettings.Amalgam.ExcludedLevels.Select(level =>level.StartsWith("level - ", StringComparison.OrdinalIgnoreCase)? level.ToLowerInvariant(): ("level - " + level).ToLowerInvariant())
            );

            return REPOLib.Modules.Levels.AllLevels
                .Where(level => !level.name.Equals(actualLevel.name, StringComparison.OrdinalIgnoreCase))
                .Where(level => !excludedSet.Contains(level.name.ToLowerInvariant()))
                .ToList();
        }

        static void AfterUnpatchAll()
        {
            roomParentLevelMap.Clear();
            initDone = false;
            actualLevel = null!;
        }
    }
}
