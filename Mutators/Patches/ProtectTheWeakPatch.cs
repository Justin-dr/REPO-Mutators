using HarmonyLib;
using Mutators.Extensions;
using Mutators.Mutators.Behaviours;
using Mutators.Mutators.Behaviours.UI;
using Mutators.Network;
using Mutators.Settings;
using Mutators.Utility;
using Photon.Pun;
using Photon.Realtime;
using REPOLib.Modules;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Mutators.Mutators.Patches
{
    internal class ProtectTheWeakPatch
    {
        private static bool initDone = false;
        private static int initialHealth = 100;
        internal const string BodyGuardId = "BodyguardId";
        internal const string TranqViewId = "tranqViewId";

        private static string? _bodyGuardId = null;
        private static IDictionary<string, object>? _clients;

        static void OnMetadataChanged(IDictionary<string, object> metadata)
        {
            string? newBodyguardId = metadata.Get<string>(BodyGuardId);
            _clients = metadata.Get<IDictionary<string, object>>("clients");

            if (newBodyguardId != _bodyGuardId)
            {
                _bodyGuardId = newBodyguardId;
                BodyguardPlayerHealthBehaviour? bodyguardPlayerHealthBehaviour = PlayerAvatar.instance?.GetComponent<BodyguardPlayerHealthBehaviour>();
                if (bodyguardPlayerHealthBehaviour != null && bodyguardPlayerHealthBehaviour)
                {
                    bodyguardPlayerHealthBehaviour.BodyguardId = _bodyGuardId;
                }

                AssignBodyguard(metadata);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerAvatar))]
        [HarmonyPatch(nameof(PlayerAvatar.Awake))]
        static void PlayerAvatarAwakePrefix(PlayerAvatar __instance)
        {
            // Far from ideal but it is possible for SteamID to be null here
            MutatorsNetworkManager.Instance.Run(Check(__instance));
        }

        private static IEnumerator Check(PlayerAvatar playerAvatar)
        {
            while (playerAvatar.steamID == null)
            {
                yield return new WaitForSeconds(0.1f);
            }

            if (playerAvatar.steamID == PlayerAvatar.instance.steamID)
            {
                playerAvatar.AddComponent<BodyguardPlayerHealthBehaviour>();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(LevelGenerator))]
        [HarmonyPatch(nameof(LevelGenerator.GenerateDone))]
        static void LevelGeneratorGenerateDonePostfix()
        {
            StartupLogic();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StatsManager))]
        [HarmonyPatch(nameof(StatsManager.ItemFetchName))]
        static void StatsManagerItemFetchNamePrefix(ref string itemName, ItemAttributes itemAttributes)
        {
            if (itemAttributes.GetComponent<TemporaryLevelItemBehaviour>())
            {
                itemName += $"({Mutators.ProtectTheWeakName})";
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
                __instance.itemName += " (Bodyguard Only)";
            }
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(ItemGun))]
        [HarmonyPatch(nameof(ItemGun.Shoot))]
        static bool ItemGunShootPrefix(ItemGun __instance)
        {
            if (__instance.GetComponent<TemporaryLevelItemBehaviour>())
            {
                bool canShoot = false;
                foreach (PhysGrabber grabber in __instance.physGrabObject.playerGrabbing)
                {
                    canShoot = grabber.playerAvatar.steamID == _bodyGuardId;
                    if (canShoot)
                    {
                        break;
                    }
                }
                return canShoot;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(NetworkManager))]
        [HarmonyPatch(nameof(NetworkManager.OnPlayerLeftRoom))]
        static void NetworkManagerOnPlayerLeftRoomPrefix(Player otherPlayer)
        {
            PlayerAvatar leavingPlayer = SemiFunc.PlayerGetFromName(otherPlayer.NickName);
            if (leavingPlayer.steamID == _bodyGuardId)
            {
                StartupLogic(leavingPlayer.steamID);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ItemBattery))]
        [HarmonyPatch(nameof(ItemBattery.RemoveFullBar))]
        static bool ItemBatteryRemoveFullBarPrefix(ItemBattery __instance)
        {
            return !__instance.GetComponent<TemporaryLevelItemBehaviour>();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(RunManager))]
        [HarmonyPriority(Priority.HigherThanNormal)]
        [HarmonyPatch(nameof(RunManager.ChangeLevel))]
        static void RunManagerChangeLevelPostfix()
        {
            if (SemiFunc.IsMultiplayer() && SemiFunc.IsNotMasterClient()) return;

            TemporaryItemUtils.DropMarkedItems(Mutators.ProtectTheWeakName);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(RunManager))]
        [HarmonyPriority(Priority.HigherThanNormal)]
        [HarmonyPatch(nameof(RunManager.UpdateLevel))]
        static void RunManagerUpdateLevelPostfix()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer()) return;

            TemporaryItemUtils.DropMarkedItems(Mutators.ProtectTheWeakName);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PunManager))]
        [HarmonyPatch(nameof(PunManager.SyncAllDictionaries))]
        static void PunManagerSyncAllDictionariesPrefix()
        {
            // It seems the game syncs all its internal dictionaries (including items) on every scene switch.
            // Patching this method with a prefix will remove all our bodyguard items from the dictionary before syncing
            // This means we are sending less data to be synced (Amazing), and our clients won't get warnings in console (wow!)
            if (SemiFunc.IsMasterClientOrSingleplayer() && SemiFunc.RunIsShop())
            {
                TemporaryItemUtils.RemoveMarkedItems(Mutators.ProtectTheWeakName);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TruckScreenText))]
        [HarmonyPatch(nameof(TruckScreenText.GotoNextLevel))]
        static void TruckScreenTextEngineStartRPCPrefix()
        {
            if (SemiFunc.IsMultiplayer() && SemiFunc.IsMasterClient())
            {
                foreach (PlayerAvatar player in SemiFunc.PlayerGetAll().Where(player => player.steamID != PlayerAvatar.instance.steamID))
                {
                    int? health = _clients.Get<IDictionary<string, object>>(player.steamID).Get<int>("originalHealth");

                    if (health == null)
                    {
                        RepoMutators.Logger.LogWarning($"Failed to restore original health for {player.playerName}");
                        continue;
                    }

                    RepoMutators.Logger.LogDebug($"Setting health to {health.Value} for {player.playerName}");
                    StatsManager.instance.SetPlayerHealth(player.steamID, health.Value, false);
                };

                PlayerAvatar.instance.GetComponent<BodyguardPlayerHealthBehaviour>()?.RestoreOriginalHealth();
            }
            
        }

        [HarmonyPostfix]
        [HarmonyPriority(Priority.LowerThanNormal)]
        [HarmonyPatch(typeof(PunManager))]
        [HarmonyPatch(nameof(PunManager.UpdateHealthRightAway))]
        static void PunManagerUpgradeHealthRightAwayPostfix(string _steamID)
        {
            PlayerAvatar playerAvatar = SemiFunc.PlayerAvatarGetFromSteamID(_steamID);
            BodyguardPlayerHealthBehaviour bodyguardPlayerHealth = playerAvatar.GetComponent<BodyguardPlayerHealthBehaviour>();
            if (bodyguardPlayerHealth != null)
            {
                bodyguardPlayerHealth.originalHealth += 20;
                bodyguardPlayerHealth.originalMaxHealth += 20;
                bodyguardPlayerHealth.UpdateHealth();
                bodyguardPlayerHealth.SendHealth();
            }
        }

        [HarmonyPostfix]
        [HarmonyPriority(Priority.VeryLow)]
        [HarmonyPatch(typeof(PlayerAvatar))]
        [HarmonyPatch(nameof(PlayerAvatar.PlayerDeathRPC))]
        static void PlayerDeathRPCPostfix()
        {
            PlayerAvatar localPlayer = PlayerAvatar.instance;
            if (_bodyGuardId == PlayerAvatar.instance.steamID && !localPlayer.deadSet)
            {
                IList<PlayerAvatar> players = SemiFunc.PlayerGetAll();
                if (players.Count <= 1) return;

                bool allWeaklingsDead = players.Where(player => player.steamID != localPlayer.steamID).All(player => player.deadSet);
                if (!allWeaklingsDead) return;

                ChatManager.instance.PossessSelfDestruction();
            }
        }

        private static PlayerAvatar PickBodyguardPlayer(string? excludedId = null)
        {
            List<PlayerAvatar> playerAvatars = SemiFunc.PlayerGetAll().Where(x => x.steamID != excludedId).ToList();
            return playerAvatars[UnityEngine.Random.RandomRangeInt(0, playerAvatars.Count)];
        }

        private static void StartupLogic(string? excludedId = null)
        {
            if (!SemiFunc.IsMasterClient()) return;
            PlayerAvatar bodyguard = PickBodyguardPlayer(excludedId);
            RepoMutators.Logger.LogDebug($"Picked {bodyguard.playerName} as the bodyguard!");


            if (Items.TryGetItemByName("Tranq gun", out var item))
            {
                GameObject? spawnedItem = Items.SpawnItem(item, bodyguard.transform.position, UnityEngine.Quaternion.identity);

                if (spawnedItem != null)
                {
                    int? viewID = spawnedItem.GetComponent<PhotonView>()?.ViewID;

                    if (viewID != null)
                    {
                        MutatorsNetworkManager.Instance.SendMetadata(BuildMeta(bodyguard, viewID.Value));

                        MutatorsNetworkManager.Instance.SendComponentForViews([viewID.Value], typeof(TemporaryLevelItemBehaviour));
                        return;
                    }
                }
            }
            RepoMutators.Logger.LogWarning("Failed to spawn Tranq gun");
            MutatorsNetworkManager.Instance.SendMetadata(BuildMeta(bodyguard));
        }

        private static void AssignBodyguard(IDictionary<string, object> metadata)
        {
            if (!string.IsNullOrEmpty(_bodyGuardId))
            {
                PlayerAvatar playerAvatar = PlayerAvatar.instance;
                if (initDone)
                {
                    MutatorDescriptionAnnouncingBehaviour mutatorDescriptionAnnouncingBehaviour = MutatorDescriptionAnnouncingBehaviour.Instance;
                    if (_bodyGuardId == playerAvatar.steamID)
                    {
                        mutatorDescriptionAnnouncingBehaviour.Text.text = metadata.Get<string>("descriptionBodyguard");
                        playerAvatar.GetComponent<BodyguardPlayerHealthBehaviour>()?.UpdateHealth();

                        MutatorsNetworkManager.Instance.Run(EquipDelayed(metadata, 1f));
                    }
                    else
                    {
                        mutatorDescriptionAnnouncingBehaviour.Text.text = metadata.Get<string>("description");
                    }
                    mutatorDescriptionAnnouncingBehaviour.ShowDescription();
                }
                else
                {
                    initDone = true;
                    if (_bodyGuardId == playerAvatar.steamID)
                    {
                        MutatorsNetworkManager.Instance.Run(LateUpdateDescription(metadata.Get<string>("descriptionBodyguard")));
                        MutatorsNetworkManager.Instance.Run(EquipDelayed(metadata));
                    }
                    else
                    {
                        MutatorsNetworkManager.Instance.Run(LateUpdateDescription(metadata.Get<string>("description")));
                    }
                }
            }
        }

        private static IEnumerator EquipDelayed(IDictionary<string, object> metadata, float delay = 3f)
        {
            yield return new WaitForSeconds(delay); // This will definitely cause issues, right?
            RepoMutators.Logger.LogDebug("I am the bodyguard");

            HandleWeapon(metadata.Get<int>(TranqViewId));
        }

        private static IEnumerator LateUpdateDescription(string description)
        {
            while (!MutatorDescriptionAnnouncingBehaviour.Instance)
            {
                yield return new WaitForSeconds(0.1f);
            }

            MutatorDescriptionAnnouncingBehaviour.Instance.Text.text = description;
        }

        private static void HandleWeapon(int tranqViewId)
        {
            PhotonView? tranqView = PhotonView.Find(tranqViewId);
            ItemEquippable? itemEquippable = tranqView?.GetComponent<ItemEquippable>();

            if (itemEquippable != null)
            {
                RepoMutators.Logger.LogDebug("Attempting to equip tranq gun");
                PlayerAvatar playerAvatar = PlayerAvatar.instance;
                Inventory inventory = Inventory.instance;
                int firstFreeSlot = inventory.GetFirstFreeInventorySpotIndex();
                if (firstFreeSlot == -1)
                {
                    InventorySpot inventorySpot = inventory.GetSpotByIndex(0);
                    inventorySpot.CurrentItem.ForceUnequip(playerAvatar.transform.position + Vector3.forward, playerAvatar.photonView.ViewID);
                    firstFreeSlot = 0;
                }

                itemEquippable.RequestEquip(firstFreeSlot, playerAvatar.photonView.ViewID);
            }
            else
            {
                RepoMutators.Logger.LogWarning($"Failed to equip bodyguard weapon - viewId: {tranqViewId} - equippable: {itemEquippable != null}");
            }
        }

        private static IDictionary<string, object> BuildMeta(PlayerAvatar bodyguard, int tranqGunId = 0)
        {
            IDictionary<string, object> meta = new Dictionary<string, object>() {
                { BodyGuardId, bodyguard.steamID },
                { "description", $"You are fragile but {bodyguard.playerName} will protect you!\n{bodyguard.playerName} dies if they are the last one standing" },
                { "descriptionBodyguard", $"You are the bodyguard, you have increased health\nYour tranq gun has infinite ammo\nYou die if you are the last one standing\nProtect your friends!" }
            };

            if (tranqGunId > 0)
            {
                meta.Add(TranqViewId, tranqGunId);
            }

            return meta.WithMutator(MutatorSettings.ProtectTheWeak.MutatorName);
        }

        private static void BeforeUnpatchAll()
        {
            initialHealth = 100;
            initDone = false;

            _bodyGuardId = null;
            _clients?.Clear();
        }
    }
}
