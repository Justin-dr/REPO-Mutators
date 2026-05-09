using HarmonyLib;
using Mutators.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mutators.Enums;
using Mutators.Managers;
using Mutators.Network;
using UnityEngine;

namespace Mutators.Mutators.Patches
{
    internal class AmalgamPatch
    {
        private static bool initDone = false;
        private static Level actualLevel = null!;
        
        static void BeforePatchAll()
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;
            MutatorManager.Instance.GameStateChanged += RemoveBrokenDoors;
        }

        private static void RemoveBrokenDoors(MutatorsGameState gameState)
        {
            if (gameState == MutatorsGameState.LevelGenerated)
            {
                MutatorsNetworkManager.Instance.Run(ScheduleRemoveBrokenDoors());
            }
        }

        private static IEnumerator ScheduleRemoveBrokenDoors()
        {
            yield return new WaitForSeconds(1);

            foreach (DirtFinderMapDoor door in UnityEngine.Object.FindObjectsByType<DirtFinderMapDoor>(FindObjectsSortMode.None))
            {
                if (!door) continue;

                PhysGrabHinge hinge = door.GetComponent<PhysGrabHinge>();
                
                if (!hinge || !hinge.broken) continue;
                    
                PhysGrabObjectImpactDetector impact = door.GetComponent<PhysGrabObjectImpactDetector>();
                if (impact)
                {
                    RepoMutators.Logger.LogDebug($"[Amalgam] removing broken door {door.transform.name}");
                    impact.DestroyObject(false);
                }
                else
                {
                    RepoMutators.Logger.LogWarning($"[Amalgam] unable to find impactDetector on {door.transform.name}");
                }
            }
        }

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
                __instance.Level.BlockObject = actualLevel.BlockObject;
                __instance.Level.MusicPreset = actualLevel.MusicPreset;
                __instance.Level.ConstantMusicPreset = actualLevel.ConstantMusicPreset;

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
            }
        }

        private static void AddNormalModules(LevelGenerator levelGenerator, Level level)
        {
            levelGenerator.ModulesNormalShuffled_1.AddRange(level.ModulesNormal1);
            levelGenerator.ModulesNormalShuffled_2.AddRange(level.ModulesNormal2);
            levelGenerator.ModulesNormalShuffled_3.AddRange(level.ModulesNormal3);

            levelGenerator.ModulesNormalShuffled_1.Shuffle();
            levelGenerator.ModulesNormalShuffled_2.Shuffle();
            levelGenerator.ModulesNormalShuffled_3.Shuffle();
        }

        private static void AddPassageModules(LevelGenerator levelGenerator, Level level)
        {
            levelGenerator.ModulesPassageShuffled_1.AddRange(level.ModulesPassage1);
            levelGenerator.ModulesPassageShuffled_2.AddRange(level.ModulesPassage2);
            levelGenerator.ModulesPassageShuffled_3.AddRange(level.ModulesPassage3);

            levelGenerator.ModulesPassageShuffled_1.Shuffle();
            levelGenerator.ModulesPassageShuffled_2.Shuffle();
            levelGenerator.ModulesPassageShuffled_3.Shuffle();
        }

        private static void AddDeadEndModules(LevelGenerator levelGenerator, Level level)
        {
            levelGenerator.ModulesDeadEndShuffled_1.AddRange(level.ModulesDeadEnd1);
            levelGenerator.ModulesDeadEndShuffled_2.AddRange(level.ModulesDeadEnd2);
            levelGenerator.ModulesDeadEndShuffled_3.AddRange(level.ModulesDeadEnd3);

            levelGenerator.ModulesDeadEndShuffled_1.Shuffle();
            levelGenerator.ModulesDeadEndShuffled_2.Shuffle();
            levelGenerator.ModulesDeadEndShuffled_3.Shuffle();
        }

        private static void AddExtractionModules(LevelGenerator levelGenerator, Level level)
        {
            levelGenerator.ModulesExtractionShuffled_1.AddRange(level.ModulesExtraction1);
            levelGenerator.ModulesExtractionShuffled_2.AddRange(level.ModulesExtraction2);
            levelGenerator.ModulesExtractionShuffled_3.AddRange(level.ModulesExtraction3);

            levelGenerator.ModulesExtractionShuffled_1.Shuffle();
            levelGenerator.ModulesExtractionShuffled_2.Shuffle();
            levelGenerator.ModulesExtractionShuffled_3.Shuffle();
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
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                MutatorManager.Instance.GameStateChanged -= RemoveBrokenDoors;
            }
            
            initDone = false;
            actualLevel = null!;
        }
    }
}
