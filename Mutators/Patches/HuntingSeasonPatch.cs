using HarmonyLib;
using Mutators.Mutators.Behaviours;
using Mutators.Network;
using Mutators.Settings;
using Photon.Pun;
using REPOLib.Modules;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mutators.Mutators.Patches
{
    internal class HuntingSeasonPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(LevelGenerator))]
        [HarmonyPatch(nameof(LevelGenerator.GenerateDone))]
        static void LevelGeneratorGenerateDonePostfix()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer() && SemiFunc.RunIsLevel())
            {
                int weaponsToSpawn = RoundDirector.instance.physGrabObjects.Count / 2;
                // Getting a shallow copy of this list since it seems to be possible for this to be
                // modified by other mods while we are looping this.
                foreach (PhysGrabObject physGrabObject in RoundDirector.instance.physGrabObjects.ToList())
                {
                    if (physGrabObject.isNonValuable) continue;

                    physGrabObject.DestroyPhysGrabObject();
                }

                RepoMutators.Logger.LogInfo($"[{MutatorSettings.HuntingSeason.MutatorName}] Spawning {weaponsToSpawn} weapons");
                Item[] possibleItems = GetPossibleItems();

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

            DropItems();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(RunManager))]
        [HarmonyPriority(Priority.HigherThanNormal)]
        [HarmonyPatch(nameof(RunManager.UpdateLevel))]
        static void RunManagerUpdateLevelPostfix()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer()) return;

            DropItems();
        }

        private static Item[] GetPossibleItems()
        {
            return Items.AllItems.Where(i => i.itemType == SemiFunc.itemType.melee || i.itemType == SemiFunc.itemType.gun).ToArray();
        }

        private static void DropItems()
        {
            Inventory inventory = Inventory.instance;
            foreach (InventorySpot inventorySpot in inventory.inventorySpots)
            {
                ItemEquippable currentItem = inventorySpot.CurrentItem;
                if (currentItem && currentItem.gameObject.GetComponent<TemporaryLevelItemBehaviour>())
                {
                    RepoMutators.Logger.LogDebug($"Dropping item: {currentItem}");
                    if (SemiFunc.IsMultiplayer())
                    {
                        currentItem.GetComponent<ItemEquippable>().ForceUnequip(inventory.playerAvatar.PlayerVisionTarget.VisionTransform.position, inventory.physGrabber.photonView.ViewID);
                    }
                    else
                    {
                        currentItem.GetComponent<ItemEquippable>().ForceUnequip(inventory.playerAvatar.PlayerVisionTarget.VisionTransform.position, -1);
                    }
                }
            }
        }
    }
}
