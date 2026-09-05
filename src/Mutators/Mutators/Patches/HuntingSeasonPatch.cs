using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Mutators.Announcements;
using Mutators.Enums;
using Mutators.Extensions;
using Mutators.Managers;
using Mutators.Mutators.Behaviours;
using Mutators.Network;
using Mutators.Settings;
using Mutators.Settings.Specific;
using Mutators.Utility;
using Photon.Pun;
using REPOLib.Modules;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Mutators.Mutators.Patches
{
    internal class HuntingSeasonPatch
    {
        static void BeforePatchAll()
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

            // User-defined Multi-Mutator settings are static, if an override for respawnTime is defined,
            // then the JSON config should be respected at all times. BepInEx config has no authority here.
            if (!HasUserRespawnTimeOverride())
            {
                MutatorSettings.HuntingSeason.EnemyRespawnTimeChanged += SendRespawnTime;
            }
            SendRespawnTime(null!, EventArgs.Empty);
        }

        static void OnMetadataChanged(IDictionary<string, object> metadata)
        {
            int respawnTime = metadata.Get<int>(HuntingSeasonSettings.RespawnTime);
            if (MutatorAnnouncingBag.Instance.TryGetAnnouncement(MutatorSettings.HuntingSeason.NamespacedName, out MutatorAnnouncement? announcement))
            {
                announcement.AddOrUpdateSegment(new MutatorAnnouncementDescriptionSegment(
                    HuntingSeasonSettings.RespawnTime,
                    10,
                    $"\nEnemy respawn time capped at {respawnTime} second{(respawnTime == 1 ? string.Empty : "s")}"
                ));
            }
        }

        private static void SendRespawnTime(object _, EventArgs args)
        {
            IDictionary<string, object> metadata = new Dictionary<string, object>(1)
            {
                { HuntingSeasonSettings.RespawnTime, MutatorSettings.HuntingSeason.EnemyRespawnTime }
            };

            MutatorsNetworkManager.Instance.SendMetadata(MutatorSettings.HuntingSeason.NamespacedName, metadata);
        }

        [HarmonyPostfix]
        [HarmonyPriority(Priority.LowerThanNormal)]
        [HarmonyPatch(typeof(EnemyDirector))]
        [HarmonyPatch(nameof(EnemyDirector.Start))]
        static void EnemyDirectorAmountSetupPostfix(EnemyDirector __instance)
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

            __instance.DisableEnemies(MutatorSettings.HuntingSeason, setup => setup.spawnObjects.All(so => {
                EnemyParent? enemyParent = so.Prefab.GetComponent<EnemyParent>();

                if (enemyParent == null)
                {
                    return true;
                }

                bool isPeeper = enemyParent.enemyName == "Peeper";

                return isPeeper || (!so.Prefab.GetComponentInChildren<EnemyHealth>()?.spawnValuable ?? false);
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


                // This can genuinely soft-lock a run if this errors, so wrapping.
                try
                {
                    // Getting a shallow copy of this list since it seems to be possible for this to be
                    // modified by other mods while we are looping this.
                    foreach (PhysGrabObject physGrabObject in RoundDirector.instance.physGrabObjects.ToList())
                    {
                        if (!physGrabObject || !physGrabObject.isValuable) continue;

                        physGrabObject.DestroyPhysGrabObject();
                    }
                }
                catch(Exception ex)
                {
                    RepoMutators.Logger.LogError($"[{Mutators.HuntingSeasonName}] Error while removing valuables: {ex}");
                }
                finally
                {
                    RepoMutators.Logger.LogDebug($"[{Mutators.HuntingSeasonName}] Spawning {weaponsToSpawn} weapons");

                    IList<LevelPoint> levelPoints = SemiFunc.LevelPointsGetAll();
                    IList<PhotonView> views = [];
                    for (int i = 0; i < weaponsToSpawn; i++)
                    {
                        LevelPoint levelPoint = levelPoints[Random.Range(0, levelPoints.Count)];
                        Item item = possibleItems[Random.Range(0, possibleItems.Length)];

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
        [HarmonyPriority(Priority.LowerThanNormal - 1)]
        [HarmonyPatch(typeof(EnemyParent))]
        [HarmonyPatch(nameof(EnemyParent.Despawn))]
        static void EnemyParentDespawnPostfix(EnemyParent __instance)
        {
            
            if (__instance.Enemy.HasHealth)
            {
                __instance.DespawnedTimer = Math.Min(__instance.DespawnedTimer, MutatorSettings.HuntingSeason.EnemyRespawnTime);
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

            TemporaryItemUtils.DropAndRemoveMarkedItems(Mutators.HuntingSeasonName);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(RunManager))]
        [HarmonyPriority(Priority.HigherThanNormal)]
        [HarmonyPatch(nameof(RunManager.UpdateLevel))]
        static void RunManagerUpdateLevelPostfix()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer()) return;

            TemporaryItemUtils.DropAndRemoveMarkedItems(Mutators.HuntingSeasonName);
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
            return Items.AllItems.Where(i => !i.prefab.Prefab.GetComponent<ValuableObject>() && (i.itemType == SemiFunc.itemType.melee || i.itemType == SemiFunc.itemType.gun)).ToArray();
        }

        private static bool HasUserRespawnTimeOverride()
        {
            IMutator huntingSeason = MutatorManager.Instance.RegisteredMutators[
                MutatorSettings.HuntingSeason.NamespacedName
            ];

            return MutatorManager.Instance.CurrentMutator is IMultiMutator { Source: MutatorSource.User } multiMutator
                   && multiMutator.SubMutators.TryGetValue(huntingSeason, out IDictionary<string, object>? overrides)
                   && overrides.ContainsKey(HuntingSeasonSettings.RespawnTime);
        }

        static void BeforeUnpatchAll()
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;
            MutatorSettings.HuntingSeason.EnemyRespawnTimeChanged -= SendRespawnTime;
        }
    }
}
