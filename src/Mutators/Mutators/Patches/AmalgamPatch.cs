using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Mutators.Enums;
using Mutators.Managers;
using Mutators.Network;
using Mutators.Settings;
using REPOLib.Modules;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mutators.Mutators.Patches
{
    internal class AmalgamPatch
    {
        private static bool initDone = false;
        
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

            foreach (DirtFinderMapDoor door in Object.FindObjectsByType<DirtFinderMapDoor>(FindObjectsSortMode.None))
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

        // TODO: Fix this in a way that it doesn't load literally all valuables into memory
        // [HarmonyPrefix]
        // [HarmonyPatch(typeof(ValuableDirector))]
        // [HarmonyPatch(nameof(ValuableDirector.SetupHost))]
        static void ValuableDirectorSetupHostPrefix(ValuableDirector __instance)
        {
            if (!MutatorSettings.Amalgam.AllowMixedValuables) return;
            
            Level currentLevel = LevelGenerator.Instance.Level;
            IEnumerable<LevelValuables> levelValuables = GetAllEligibleLevels(currentLevel)
                .SelectMany(level => level.ValuablePresets)
                .Where(valuablePreset => valuablePreset && !currentLevel.ValuablePresets.Contains(valuablePreset))
                .Distinct();

            foreach (LevelValuables levelValuablesPreset in levelValuables)
            {
                __instance.tinyValuables.AddRange(levelValuablesPreset.tiny);
                __instance.smallValuables.AddRange(levelValuablesPreset.small);
                __instance.mediumValuables.AddRange(levelValuablesPreset.medium);
                __instance.bigValuables.AddRange(levelValuablesPreset.big);
                __instance.wideValuables.AddRange(levelValuablesPreset.wide);
                __instance.tallValuables.AddRange(levelValuablesPreset.tall);
                __instance.veryTallValuables.AddRange(levelValuablesPreset.veryTall);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(LevelGenerator))]
        [HarmonyPatch(nameof(LevelGenerator.PlanModule))]
        static void LevelGeneratorPlanModulePrefix(LevelGenerator __instance)
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;
            if (!initDone)
            {
                RepoMutators.Logger.LogInfo("[Amalgam] Building level from the following available levels:");
                foreach (Level level in GetAllEligibleLevels(__instance.Level))
                {
                    RepoMutators.Logger.LogInfo($"[Amalgam] {level.name}");
                    AddNormalModules(__instance, level);
                    AddPassageModules(__instance, level);
                    AddDeadEndModules(__instance, level);
                    AddExtractionModules(__instance, level);
                    AddMegaModules(__instance,  level);
                }

                initDone = true;
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
        
        private static void AddMegaModules(LevelGenerator levelGenerator, Level level)
        {
            levelGenerator.ModulesMegaShuffled_1.AddRange(level.ModulesMega1);
            levelGenerator.ModulesMegaShuffled_2.AddRange(level.ModulesMega2);
            levelGenerator.ModulesMegaShuffled_3.AddRange(level.ModulesMega3);

            levelGenerator.ModulesMegaShuffled_1.Shuffle();
            levelGenerator.ModulesMegaShuffled_2.Shuffle();
            levelGenerator.ModulesMegaShuffled_3.Shuffle();
        }

        private static IList<Level> GetAllEligibleLevels(Level currentLevel)
        {
            ISet<string> excludedSet = new HashSet<string>(
                MutatorSettings.Amalgam.ExcludedLevels.Select(level =>level.StartsWith("level - ", StringComparison.OrdinalIgnoreCase)? level.ToLowerInvariant(): ("level - " + level).ToLowerInvariant())
            );

            return Levels.AllLevels
                .Where(level => !level.name.Equals(currentLevel.name, StringComparison.OrdinalIgnoreCase))
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
        }
    }
}
