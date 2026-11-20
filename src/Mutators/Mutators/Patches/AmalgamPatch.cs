using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Mutators.Enums;
using Mutators.Managers;
using Mutators.Network;
using Mutators.Providers.Random;
using Mutators.Settings;
using REPOLib.Modules;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mutators.Mutators.Patches
{
    internal class AmalgamPatch
    {
        private static bool modulesInitDone;
        private static bool valuablesInitDone;
        
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
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(LevelGenerator))]
        [HarmonyPatch(nameof(LevelGenerator.PlanModule))]
        static void LevelGeneratorPlanModulePrefix(LevelGenerator __instance)
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;
            if (!modulesInitDone)
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

                modulesInitDone = true;
            }
        }
        
        // Patching the enumerator, otherwise this patch doesn't get triggered.
        // Likely caused by the method getting inlined.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ValuableDirector))]
        [HarmonyPatch(nameof(ValuableDirector.SetupHost), MethodType.Enumerator)]
        static void ValuableDirectorSetupHostPrefix()
        {
            if (valuablesInitDone) return;
            valuablesInitDone = true;

            if (!MutatorSettings.Amalgam.AllowMixedValuables) return;

            ValuableDirector valuableDirector = ValuableDirector.instance;
            Level currentLevel = LevelGenerator.Instance.Level;
            List<LevelValuables> levelValuables = GetAllEligibleLevels(currentLevel)
                .SelectMany(level => level.ValuablePresets)
                .Where(valuablePreset => valuablePreset && !currentLevel.ValuablePresets.Contains(valuablePreset))
                .Distinct()
                .ToList();

            if (levelValuables.Count == 0) return;

            List<PrefabRef> tiny = levelValuables.SelectMany(v => v.tiny).ToList();
            List<PrefabRef> small = levelValuables.SelectMany(v => v.small).ToList();
            List<PrefabRef> medium = levelValuables.SelectMany(v => v.medium).ToList();
            List<PrefabRef> big = levelValuables.SelectMany(v => v.big).ToList();
            List<PrefabRef> wide = levelValuables.SelectMany(v => v.wide).ToList();
            List<PrefabRef> tall = levelValuables.SelectMany(v => v.tall).ToList();
            List<PrefabRef> veryTall = levelValuables.SelectMany(v => v.veryTall).ToList();
            
            // These will all round down, which is probably fine.
            int averageTiny = tiny.Count / levelValuables.Count;
            int averageSmall = small.Count / levelValuables.Count;
            int averageMedium = medium.Count / levelValuables.Count;
            int averageBig = big.Count / levelValuables.Count;
            int averageWide = wide.Count / levelValuables.Count;
            int averageTall = tall.Count / levelValuables.Count;
            int averageVeryTall = veryTall.Count / levelValuables.Count;
            
            RepoMutators.Logger.LogDebug($"[Amalgam] Adding valuables from {levelValuables.Count} other valuable presets: {string.Join(", ", levelValuables.Select(v => v.name))}");

            AddRandomValuables(nameof(valuableDirector.tinyValuables), valuableDirector.tinyValuables, tiny, averageTiny);
            AddRandomValuables(nameof(valuableDirector.smallValuables), valuableDirector.smallValuables, small, averageSmall);
            AddRandomValuables(nameof(valuableDirector.mediumValuables), valuableDirector.mediumValuables, medium, averageMedium);
            AddRandomValuables(nameof(valuableDirector.bigValuables), valuableDirector.bigValuables, big, averageBig);
            AddRandomValuables(nameof(valuableDirector.wideValuables), valuableDirector.wideValuables, wide, averageWide);
            AddRandomValuables(nameof(valuableDirector.tallValuables), valuableDirector.tallValuables, tall, averageTall);
            AddRandomValuables(nameof(valuableDirector.veryTallValuables), valuableDirector.veryTallValuables, veryTall, averageVeryTall);
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

        private static void AddRandomValuables(string category, List<PrefabRef> target, List<PrefabRef> pool, int amount)
        {
            List<PrefabRef> picked = TakeRandomFromPool(pool, amount);
            target.AddRange(picked);

            string pickedValuableNames = picked.Count > 0 ? string.Join(", ", picked.Select(p => p.PrefabName)) : "none";
            RepoMutators.Logger.LogDebug($"[Amalgam] Added {picked.Count}/{pool.Count} {category}: {pickedValuableNames}");
        }

        private static List<PrefabRef> TakeRandomFromPool(List<PrefabRef> pool, int amount)
        {
            IRandomProvider random = DI.Container.Resolve<IRandomProvider>();
            int amountToTake = Math.Min(amount, pool.Count);
            
            List<PrefabRef> picked = new List<PrefabRef>(amountToTake);
            for (int i = 0; i < amountToTake; i++)
            {
                int index = random.RandomRangeInt(0, pool.Count);
                picked.Add(pool[index]);
                pool.RemoveAt(index);
            }
            return picked;
        }
        
        static void AfterUnpatchAll()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                MutatorManager.Instance.GameStateChanged -= RemoveBrokenDoors;
            }

            modulesInitDone = false;
            valuablesInitDone = false;
        }
    }
}
