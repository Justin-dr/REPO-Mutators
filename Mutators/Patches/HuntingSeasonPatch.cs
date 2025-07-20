using HarmonyLib;
using Mutators.Extensions;
using Mutators.Mutators.Behaviours;
using Mutators.Network;
using Mutators.Utility;
using Photon.Pun;
using REPOLib.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mutators.Mutators.Patches
{
    internal class HuntingSeasonPatch
    {
        [HarmonyPostfix]
        [HarmonyPriority(Priority.LowerThanNormal)]
        [HarmonyPatch(typeof(EnemyDirector))]
        [HarmonyPatch(nameof(EnemyDirector.Start))]
        static void EnemyDirectorAmountSetupPostfix(EnemyDirector __instance)
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

            __instance.DisableEnemies(setup => setup.spawnObjects.All(so => {
                EnemyParent? enemyParent = so.GetComponent<EnemyParent>();

                if (enemyParent == null)
                {
                    return true;
                }

                bool isPeeper = enemyParent.enemyName == "Peeper";

                return isPeeper || (!so.GetComponentInChildren<EnemyHealth>()?.spawnValuable ?? false);
            }));
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StatsManager))]
        [HarmonyPatch(nameof(StatsManager.ItemFetchName))]
        static void StatsManagerItemFetchNamePrefix(ref string itemName, ItemAttributes itemAttributes)
        {
            if (itemAttributes.GetComponent<TemporaryLevelItemBehaviour>())
            {
                itemName += $"({Mutators.HuntingSeasonName})";
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(LevelGenerator))]
        [HarmonyPatch(nameof(LevelGenerator.GenerateDone))]
        static void LevelGeneratorGenerateDonePostfix()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer() && SemiFunc.RunIsLevel())
            {
                int weaponsToSpawn = RoundDirector.instance.physGrabObjects.Count / 2;

                Item[] possibleItems = GetPossibleItems();
                if (possibleItems.Length == 0)
                {
                    RepoMutators.Logger.LogWarning("No eligable weapons found");
                    RepoMutators.Logger.LogWarning("Valuables will not removed in order to prevent softlocks");
                    return;
                }

                // Getting a shallow copy of this list since it seems to be possible for this to be
                // modified by other mods while we are looping this.
                foreach (PhysGrabObject physGrabObject in RoundDirector.instance.physGrabObjects.ToList())
                {
                    if (physGrabObject.isNonValuable) continue;

                    physGrabObject.DestroyPhysGrabObject();
                }

                RepoMutators.Logger.LogDebug($"[{Mutators.HuntingSeasonName}] Spawning {weaponsToSpawn} weapons");

                IList<LevelPoint> levelPoints = SemiFunc.LevelPointsGetAll();
                IList<PhotonView> views = [];
                for (int i = 0; i < weaponsToSpawn; i++)
                {
                    LevelPoint levelPoint = levelPoints[UnityEngine.Random.Range(0, levelPoints.Count)];
                    Item item = possibleItems[UnityEngine.Random.Range(0, possibleItems.Length)];

                    Vector3 position = levelPoint.transform.position;
                    position.y += 2;
                    GameObject? itemObject = Items.SpawnItem(item, position, Quaternion.identityQuaternion);
                    if (itemObject == null) continue;

                    itemObject.AddComponent<TemporaryLevelItemBehaviour>();
                    PhotonView view = itemObject.GetComponent<PhotonView>();

                    if (view)
                    {
                        views.Add(view);
                    }
                }

                MutatorsNetworkManager.Instance.SendComponentForViews(
                    views.Select(x => x.ViewID).ToArray(),
                    typeof(TemporaryLevelItemBehaviour)
                );
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemAttributes))]
        [HarmonyPatch(nameof(ItemAttributes.Start))]
        static void ItemAttributesStartPostfix(ItemAttributes __instance)
        {
            TemporaryLevelItemBehaviour levelChangeBehaviour = __instance.gameObject.GetComponent<TemporaryLevelItemBehaviour>();
            if (levelChangeBehaviour)
            {
                __instance.itemName += " (Temporary)";
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(EnemyParent))]
        [HarmonyPatch(nameof(EnemyParent.Despawn))]
        static void EnemyParentDespawnPostfix(EnemyParent __instance)
        {
            if (__instance.Enemy.HasHealth && __instance.DespawnedTimer > 10)
            {
                __instance.DespawnedTimer = 10;
                //Unlimited valuable spawns
                __instance.Enemy.Health.spawnValuableCurrent = 0;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(RunManager))]
        [HarmonyPriority(Priority.HigherThanNormal)]
        [HarmonyPatch(nameof(RunManager.ChangeLevel))]
        static void RunManagerChangeLevelPostfix()
        {
            if (SemiFunc.IsMultiplayer() && SemiFunc.IsNotMasterClient()) return;

            TemporaryItemUtils.DropMarkedItems(Mutators.HuntingSeasonName);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(RunManager))]
        [HarmonyPriority(Priority.HigherThanNormal)]
        [HarmonyPatch(nameof(RunManager.UpdateLevel))]
        static void RunManagerUpdateLevelPostfix()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer()) return;

            TemporaryItemUtils.DropMarkedItems(Mutators.HuntingSeasonName);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PunManager))]
        [HarmonyPatch(nameof(PunManager.SyncAllDictionaries))]
        static void PunManagerSyncAllDictionariesPrefix()
        {
            // It seems the game syncs all its internal dictionaries (including items) on every scene switch.
            // Patching this method with a prefix will remove all our hunting season items from the dictionary before syncing
            // This means we are sending less data to be synced (Amazing), and our clients won't get warnings in console (wow!)
            if (SemiFunc.IsMasterClientOrSingleplayer() && SemiFunc.RunIsShop())
            {
                TemporaryItemUtils.RemoveMarkedItems(Mutators.HuntingSeasonName);
            }
        }

        private static Item[] GetPossibleItems()
        {
            return Items.AllItems.Where(i => !i.prefab.GetComponent<ValuableObject>() && (i.itemType == SemiFunc.itemType.melee || i.itemType == SemiFunc.itemType.gun)).ToArray();
        }
    }
}
