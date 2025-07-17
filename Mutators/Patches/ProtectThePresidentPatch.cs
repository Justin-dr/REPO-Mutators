using HarmonyLib;
using Mutators.Extensions;
using Mutators.Mutators.Behaviours.UI;
using Mutators.Network;
using Mutators.Settings;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

namespace Mutators.Mutators.Patches
{
    internal class ProtectThePresidentPatch
    {
        private const string PresidentId = "presidentId";
        private static bool _presidentAlive = true;
        private static bool _failureMessageSent = false;

        private static string? _presidentId;

        static void OnMetadataChanged(IDictionary<string, object> metadata)
        {
            string? newPresidentId = metadata.Get<string>(PresidentId);

            if (_presidentId != newPresidentId)
            {
                _presidentId = newPresidentId;
                UpdatePresidentHealth(_presidentId);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(LevelGenerator))]
        [HarmonyPatch(nameof(LevelGenerator.GenerateDone))]
        static void LevelGeneratorGenerateDonePostfix()
        {
            _presidentAlive = true;
            _failureMessageSent = false;
            if (SemiFunc.IsMultiplayer() && SemiFunc.IsMasterClient())
            {
                PlayerAvatar president = PickPresidentPlayer();
                RepoMutators.Logger.LogDebug($"Picked {president.playerName} as the president!");

                SendPresidentMeta(president);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(NetworkManager))]
        [HarmonyPatch(nameof(NetworkManager.OnPlayerLeftRoom))]
        static void NetworkManagerOnPlayerLeftRoomPrefix(Player otherPlayer)
        {
            PlayerAvatar leavingPlayer = SemiFunc.PlayerGetFromName(otherPlayer.NickName);
            if (leavingPlayer.steamID == _presidentId)
            {
                if (SemiFunc.IsMultiplayer() && SemiFunc.IsMasterClient())
                {
                    PlayerAvatar president = PickPresidentPlayer();
                    SendPresidentMeta(president);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerAvatar))]
        [HarmonyPatch(nameof(PlayerAvatar.PlayerDeathRPC))]
        static void PlayerAvatarPlayerDeathRPCPostfix(PlayerAvatar __instance, int enemyIndex)
        {
            // If we are the president, do nothing. Else, die.
            if (__instance.steamID == _presidentId && __instance.steamID != PlayerAvatar.instance.steamID)
            {
                _presidentAlive = false;
                PlayerAvatar? presidentAvatar = SemiFunc.PlayerGetFromSteamID(_presidentId);
                SemiFunc.UIFocusText($"President {(presidentAvatar?.playerName != null ? $"{presidentAvatar.playerName} " : "")} has died.", Color.white, AssetManager.instance.colorYellow);
                ChatManager.instance.PossessSelfDestruction();
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ChatManager))]
        [HarmonyPatch(nameof(ChatManager.PossessChat))]
        static void ChatManagerPossessChatPrefix(ChatManager.PossessChatID _possessChatID, ref string message)
        {
            if (!_presidentAlive && !_failureMessageSent && _possessChatID == ChatManager.PossessChatID.SelfDestruct)
            {
                PlayerAvatar playerAvatar = SemiFunc.PlayerGetFromSteamID(_presidentId);
                message = $"We have failed {playerAvatar?.playerName ?? " the president"}";
                _failureMessageSent = true;
            }
        }

        [HarmonyPostfix]
        [HarmonyPriority(Priority.VeryLow)]
        [HarmonyPatch(typeof(PlayerHealth))]
        [HarmonyPatch(nameof(PlayerHealth.UpdateHealthRPC))]
        static void PlayerHealthUpdateHealthRPCPostfix(PlayerHealth __instance)
        {
            if (__instance.playerAvatar.steamID != _presidentId || _presidentId == PlayerAvatar.instance.steamID) return;

            UpdatePresidentHealth(_presidentId);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(LoadingUI))]
        [HarmonyPatch(nameof(LoadingUI.StopLoading))]
        static void InitializePresidentHealth()
        {
            if (!SemiFunc.RunIsLevel())
            {
                TargetPlayerAnnouncingBehaviour? targetPlayerAnnouncingBehaviour = TargetPlayerAnnouncingBehaviour.instance;
                if (targetPlayerAnnouncingBehaviour)
                {
                    targetPlayerAnnouncingBehaviour.Text.text = string.Empty;
                }
                return;
            };

            if (_presidentId != null && _presidentId != PlayerAvatar.instance.steamID)
            {
                MutatorsNetworkManager.Instance.Run(InitializePresidentHealthEnumerator(_presidentId));
            }
        }

        private static void UpdatePresidentHealth(string presidentId)
        {
            TargetPlayerAnnouncingBehaviour targetPlayerAnnouncingBehaviour = TargetPlayerAnnouncingBehaviour.instance;
            if (!targetPlayerAnnouncingBehaviour) return;

            RepoMutators.Logger.LogDebug("Updating President Health");
            if (presidentId == PlayerAvatar.instance.steamID)
            {
                targetPlayerAnnouncingBehaviour.Text.text = string.Empty;
            }
            else
            {
                targetPlayerAnnouncingBehaviour.Text.text = BuildPresidentText(presidentId);
            }
        }

        internal static string BuildPresidentText(string presidentId)
        {
            PlayerAvatar playerAvatar = SemiFunc.PlayerAvatarGetFromSteamID(presidentId);
            return $"{playerAvatar.playerName ?? "President"}: {playerAvatar.playerHealth.health}/{playerAvatar.playerHealth.maxHealth}";
        }

        private static System.Collections.IEnumerator InitializePresidentHealthEnumerator(string presidentId)
        {
            while (TargetPlayerAnnouncingBehaviour.instance == null)
            {
                yield return new WaitForSeconds(0.5f);
            }
            UpdatePresidentHealth(presidentId);
        }

        private static PlayerAvatar PickPresidentPlayer()
        {
            List<PlayerAvatar> playerAvatars = SemiFunc.PlayerGetAll();
            return playerAvatars[Random.RandomRangeInt(0, playerAvatars.Count)];
        }

        private static void SendPresidentMeta(PlayerAvatar president)
        {
            MutatorsNetworkManager.Instance.SendMetadata(
                new Dictionary<string, object>() { { PresidentId, president.steamID } }.WithMutator(MutatorSettings.ProtectThePresident.MutatorName)
            );
        }

        private static void AfterUnpatchAll()
        {
            _presidentId = null;
        }
    }
}
