using HarmonyLib;
using Mutators.Mutators.Behaviours;
using Mutators.Settings;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Mutators.Mutators.Patches
{
    internal class DuckThisPatch
    {
        private const string Ducky = "Apex Predator";

        [HarmonyPostfix]
        [HarmonyPriority(Priority.VeryLow)]
        [HarmonyPatch(typeof(EnemyDirector))]
        [HarmonyPatch(nameof(EnemyDirector.AmountSetup))]
        static void EnemyDirectorAmountSetupPostfix(EnemyDirector __instance)
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

            IList<EnemySetup> enemyList = __instance.enemyList;
            bool hasApexPredator = enemyList.Any(setup => setup.spawnObjects.Select(so => so.Prefab.GetComponent<EnemyParent>()).Any(ep => ep != null && ep.enemyName == Ducky));

            if (hasApexPredator) return;

            EnemySetup? duckSetup = REPOLib.Modules.Enemies.AllEnemies.Where(enemySetup => enemySetup.name == "Enemy - Duck").FirstOrDefault();
            if (duckSetup)
            {
                IList<EnemySetup> setups = enemyList.Where(setup =>
                {
                    IEnumerable<EnemyParent> enemyParents = setup.spawnObjects
                        .Select(so => so.Prefab.GetComponent<EnemyParent>())
                        .Where(ep => ep != null);

                    return enemyParents.All(ep => ep.difficulty == EnemyParent.Difficulty.Difficulty1) && enemyParents.All(ep => ep.enemyName != Ducky);
                }).ToList();

                if (setups.Count == 0)
                {
                    RepoMutators.Logger.LogWarning($"No suitable enemies found to replace with {Ducky}");
                    return;
                };

                EnemySetup setupToRemove = setups[Random.RandomRangeInt(0, setups.Count)];
                if (!enemyList.Remove(setupToRemove)) return;

                RepoMutators.Logger.LogDebug($"[{MutatorSettings.DuckThis.MutatorName}] {setupToRemove.name} was removed in favor of {Ducky}");
                enemyList.Add(duckSetup);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EnemyDuck))]
        [HarmonyPatch(nameof(EnemyDuck.Update))]
        static void EnemyDuckUpdatePrefix(EnemyDuck __instance)
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                DuckThisBehaviour noticeBehaviour = __instance.GetOrAddComponent<DuckThisBehaviour>();

                if (__instance.currentState == EnemyDuck.State.GoToPlayer || __instance.currentState == EnemyDuck.State.GoToPlayerOver || __instance.currentState == EnemyDuck.State.GoToPlayerUnder)
                {
                    if (noticeBehaviour.CanNotice())
                    {
                        __instance.UpdateState(EnemyDuck.State.AttackStart);
                    }
                }
                if (__instance.currentState == EnemyDuck.State.DeTransform)
                {
                    __instance.playerTarget = null;
                    __instance.UpdateState(EnemyDuck.State.Idle);
                    noticeBehaviour.NoticeCooldown = MutatorSettings.DuckThis.AggroCooldown;
                }
            }
        }
    }
}
