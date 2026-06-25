using HarmonyLib;
using Mutators.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using Mutators.Managers;

namespace Mutators.Mutators.Patches
{
    internal class ThereCanOnlyBeOnePatch
    {
        private const string Ducky = "Apex Predator";
        private const string EnemyGroupPrefix = "Enemy Group - ";

        [HarmonyPostfix]
        [HarmonyPriority(Priority.VeryLow)]
        [HarmonyPatch(typeof(EnemyDirector))]
        [HarmonyPatch(nameof(EnemyDirector.AmountSetup))]
        static void EnemyDirectorAmountSetupPostfix(EnemyDirector __instance)
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

            IList<string> excludedEnemies = MutatorSettings.ThereCanOnlyBeOne.ExcludedEnemies;
            bool groupsAllowed = RunManager.instance.levelsCompleted >= MutatorSettings.ThereCanOnlyBeOne.GroupSpawnsThreshold;

            IEnumerable<EnemySetup> allEnemies = __instance.enemiesDifficulty1
                .Concat(__instance.enemiesDifficulty2)
                .Concat(__instance.enemiesDifficulty3)
                .Where(setup => groupsAllowed || !setup.name.StartsWith(EnemyGroupPrefix));

            
            if (!excludedEnemies.Contains(Ducky) && MutatorManager.Instance.HasCurrentMutator(MutatorSettings.DuckThis.NamespacedName) && EnemiesContainDucky(allEnemies))
            {
                RepoMutators.Logger.LogDebug($"[{Mutators.ThereCanOnlyBeOneName}] Applying special logic for {Mutators.DuckThisName}");
                List<EnemySetup> setups = allEnemies.Where(setup => setup.spawnObjects.All(so => so.Prefab.GetComponent<EnemyParent>()?.enemyName == Ducky)).ToList();
                ApplyChosenEnemy(__instance, setups[UnityEngine.Random.RandomRangeInt(0, setups.Count)]);
                return;
            }

            IList<EnemySetup> availableEnemies = allEnemies
                .Where(setup => setup.spawnObjects.All(so =>!excludedEnemies.Any(excluded => excluded.Equals(so.Prefab.GetComponent<EnemyParent>()?.enemyName, StringComparison.OrdinalIgnoreCase))))
                .ToList();


            if (availableEnemies.Count == 0)
            {
                RepoMutators.Logger.LogWarning($"[{Mutators.ThereCanOnlyBeOneName}] Based on your config, there were no enemies that could be spawned for the {Mutators.ThereCanOnlyBeOneName} Mutator!");
                return;
            };

            EnemySetup theChosenOne = availableEnemies[UnityEngine.Random.RandomRangeInt(0, availableEnemies.Count)];
            ApplyChosenEnemy(__instance, theChosenOne);
        }

        private static void ApplyChosenEnemy(EnemyDirector enemyDirector, EnemySetup enemySetup)
        {
            enemyDirector.enemyList.Clear();
            enemyDirector.enemyList.Add(enemySetup);
            for (int i = 0; i < enemyDirector.totalAmount; i++) // Prevent index out of bound
            {
                enemyDirector.enemyList.Add(enemySetup);
            }
        }

        private static bool EnemiesContainDucky(IEnumerable<EnemySetup> enemies)
        {
            return enemies.Any(setup => setup.spawnObjects.All(so => so.Prefab.GetComponent<EnemyParent>()?.enemyName == Ducky));
        }
    }
}
