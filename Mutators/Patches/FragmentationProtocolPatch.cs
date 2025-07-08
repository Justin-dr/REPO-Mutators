using HarmonyLib;
using Mutators.Mutators.Behaviours;
using Mutators.Network;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.Events;

namespace Mutators.Mutators.Patches
{
    internal class FragmentationProtocolPatch
    {
        private static HashSet<EnemyParent> fragments = new HashSet<EnemyParent>();

        [HarmonyPostfix]
        [HarmonyPatch(typeof(EnemyHealth))]
        [HarmonyPatch(nameof(EnemyHealth.Awake))]
        static void EnemyHealthAwakePostfix(EnemyHealth __instance)
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

            RepoMutators.Logger.LogInfo($"Checking fragment");

            if (IsFragment(__instance.enemy.EnemyParent)) return;

            RepoMutators.Logger.LogInfo($"Enemy was not a fragment");

            if (REPOLib.Modules.Enemies.TryGetEnemyByName(__instance.enemy.EnemyParent.enemyName, out EnemySetup? enemySetup))
            {
                List<EnemyParent>? enemyParents = REPOLib.Modules.Enemies.SpawnEnemy(enemySetup, __instance.enemy.EnemyParent.transform.position, UnityEngine.Quaternion.identity);

                if (enemyParents == null || enemyParents.Count == 0) return;

                FragmentationProtocolBehaviour fragmentationProtocolBehaviour = __instance.enemy.EnemyParent.AddComponent<FragmentationProtocolBehaviour>();

                foreach (EnemyParent enemyParent in enemyParents)
                {
                    RepoMutators.Logger.LogInfo($"Adding fragment: {enemyParent.transform.name}");
                    enemyParent.DespawnedTimer = float.MaxValue;
                    fragmentationProtocolBehaviour.AddFragmentation(enemyParent);
                    fragments.Add(enemyParent);
                    MutatorsNetworkManager.Instance.SendScaleChange(enemyParent.photonView.ViewID, 0.6f);

                    RepoMutators.Logger.LogInfo($"Added fragment: {enemyParent.transform.name}");
                }

                __instance.onDeath.AddListener(new UnityAction(() => MakeBabies(__instance.enemy.EnemyParent)));
            }
        }

        private static void MakeBabies(EnemyParent enemyParent)
        {
            if (IsFragment(enemyParent)) return;

            FragmentationProtocolBehaviour fragmentationProtocolBehaviour = enemyParent.GetComponent<FragmentationProtocolBehaviour>();
            if (fragmentationProtocolBehaviour)
            {
                foreach (EnemyParent fragmentation in fragmentationProtocolBehaviour.GetFragmentations())
                {
                    fragmentation.DespawnedTimer = 0;
                    enemyParent.Enemy.EnemyTeleported(enemyParent.transform.position);
                }
            }
        }

        private static bool IsFragment(EnemyParent enemyParent)
        {
            return fragments.Contains(enemyParent);
        }
    }
}
