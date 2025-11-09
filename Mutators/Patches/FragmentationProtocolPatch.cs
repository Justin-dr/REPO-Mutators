using HarmonyLib;
using Mutators.Mutators.Behaviours;
using Mutators.Network;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.Events;

namespace Mutators.Mutators.Patches
{
    internal class FragmentationProtocolPatch
    {
        private static readonly IDictionary<EnemyParent, EnemyParent> fragmentsParentMap = new Dictionary<EnemyParent, EnemyParent>();

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Enemy))]
        [HarmonyPatch(nameof(Enemy.Start))]
        static void EnemyHealthAwakePostfix(Enemy __instance)
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

            RepoMutators.Logger.LogInfo($"Checking fragment");

            if (!__instance.HasHealth || IsFragment(__instance.EnemyParent)) return;

            RepoMutators.Logger.LogInfo($"Enemy was not a fragment");

            EnemySetup? enemySetup = REPOLib.Modules.Enemies.AllEnemies.Where(x => __instance.EnemyParent.enemyName.Equals(x.name, System.StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (enemySetup)
            {
                FragmentationProtocolBehaviour fragmentationProtocolBehaviour = __instance.EnemyParent.GetOrAddComponent<FragmentationProtocolBehaviour>();

                __instance.Health.onDeath.AddListener(new UnityAction(() => MakeBabies(__instance.EnemyParent, enemySetup, fragmentationProtocolBehaviour)));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(EnemyParent))]
        [HarmonyPatch(nameof(EnemyParent.Despawn))]
        static void EnemyHealthAwakePostfix(EnemyParent __instance)
        {
            if (SemiFunc.IsMasterClientOrSingleplayer() && fragmentsParentMap.ContainsKey(__instance))
            {
                RepoMutators.Logger.LogInfo("Despawned " + __instance.enemyName);
                __instance.DespawnedTimer = float.MaxValue;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EnemyParent))]
        [HarmonyPatch(nameof(EnemyParent.SpawnedTimerPause))]
        static void EnemyHealthAwakePostfix(EnemyParent __instance, ref float _time)
        {
            if (SemiFunc.IsMasterClientOrSingleplayer() && fragmentsParentMap.ContainsKey(__instance))
            {
                _time = 0f;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EnemyParent))]
        [HarmonyPatch(nameof(EnemyParent.Spawn))]
        static bool EnemyParentSpawnPrefix(EnemyParent __instance)
        {
            if (fragmentsParentMap.TryGetValue(__instance, out EnemyParent actualParent) && actualParent.Enemy.HasHealth)
            {
                FragmentationProtocolBehaviour fragmentationProtocolBehaviour = actualParent.GetComponent<FragmentationProtocolBehaviour>();
                RepoMutators.Logger.LogInfo($"In Fragment Window: {fragmentationProtocolBehaviour.IsInFragmentWindow} - {fragmentationProtocolBehaviour.FragmentWindow}");
                if (fragmentationProtocolBehaviour && !fragmentationProtocolBehaviour.IsInFragmentWindow)
                {
                    __instance.DespawnedTimer = RoundDirector.instance.allExtractionPointsCompleted ? 30f : float.MaxValue;
                    return false;
                }
            }
            return true;
        }

        private static void MakeBabies(EnemyParent enemyParent, EnemySetup enemySetup, FragmentationProtocolBehaviour fragmentationProtocolBehaviour)
        {
            fragmentationProtocolBehaviour.FragmentWindow = 0.5f;
            if (fragmentationProtocolBehaviour.GetFragmentations().Count == 0)
            {
                List<EnemyParent>? enemyParents = REPOLib.Modules.Enemies.SpawnEnemy(enemySetup, enemyParent.Enemy.CenterTransform.position, UnityEngine.Quaternion.identity, false);

                RepoMutators.Logger.LogInfo($"Created {enemyParents?.Count ?? 0} fragments");

                if (enemyParents == null || enemyParents.Count == 0) return;

                foreach (EnemyParent fragment in enemyParents)
                {
                    RepoMutators.Logger.LogInfo($"Adding fragment: {fragment.transform.name}");
                    MutatorsNetworkManager.Instance.SendScaleChange(fragment.photonView.ViewID, 0.6f);
                    fragmentationProtocolBehaviour.AddFragmentation(fragment);
                    fragmentsParentMap.Add(fragment, enemyParent);

                    RepoMutators.Logger.LogInfo($"Added fragment: {fragment.transform.name}");
                }
            }
            else
            {
                foreach (EnemyParent fragment in fragmentationProtocolBehaviour.GetFragmentations())
                {
                    fragment.DespawnedTimer = 0;
                    fragment.Spawn();
                    fragment.Enemy.EnemyTeleported(enemyParent.transform.position);
                }
            }
        }

        private static bool IsFragment(EnemyParent enemyParent)
        {
            return fragmentsParentMap.ContainsKey(enemyParent);
        }

        static void AfterUnpatchAll()
        {
            fragmentsParentMap.Clear();
        }
    }
}
