using HarmonyLib;
using Mutators.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

namespace Mutators.Mutators.Patches
{
    internal class ThereCanOnlyBeOnePatch
    {
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

            IList<EnemySetup> availableEnemies = __instance.enemiesDifficulty1
                .Concat(__instance.enemiesDifficulty2)
                .Concat(__instance.enemiesDifficulty3)
                .Where(setup => groupsAllowed || !setup.name.StartsWith(EnemyGroupPrefix))
                .Where(setup => setup.spawnObjects.All(so =>!excludedEnemies.Any(excluded => excluded.Equals(so.GetComponent<EnemyParent>()?.enemyName, StringComparison.OrdinalIgnoreCase))))
                .ToList();


            if (availableEnemies.Count == 0)
            {
                RepoMutators.Logger.LogWarning($"Based on your config, there were no enemies that could be spawned for the {MutatorSettings.ThereCanOnlyBeOne.MutatorName} Mutator!");
                return;
            };

            __instance.enemyList.Clear();
            EnemySetup theChosenOne = availableEnemies[UnityEngine.Random.RandomRangeInt(0, availableEnemies.Count)];
            __instance.enemyList.Add(theChosenOne);
            for (int i = 0; i < __instance.totalAmount; i++) // Prevent index out of bound
            {
                __instance.enemyList.Add(theChosenOne);
            }
        }
    }
}
