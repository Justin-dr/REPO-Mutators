using HarmonyLib;
using Mutators.Managers;
using Mutators.Mutators.Behaviours;
using Mutators.Network;
using Mutators.Settings;
using Photon.Pun;
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

                MutatorsNetworkManager.Instance.SendMetadata(new Dictionary<string, string>() { { PresidentId, president.steamID } });
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(NetworkManager))]
        [HarmonyPatch(nameof(NetworkManager.OnPlayerLeftRoom))]
        static void NetworkManagerOnPlayerLeftRoomPrefix(Player otherPlayer)
        {
            PlayerAvatar leavingPlayer = SemiFunc.PlayerGetFromName(otherPlayer.NickName);
            if (leavingPlayer.steamID == MutatorManager.Instance.Metadata[PresidentId])
            {
                MutatorManager.Instance.OnMetadataChanged += OnMetadataChanged;

                if (SemiFunc.IsMultiplayer() && SemiFunc.IsMasterClient())
                {
                    PlayerAvatar president = PickPresidentPlayer();
                    MutatorsNetworkManager.Instance.SendMetadata(new Dictionary<string, string>() { { PresidentId, president.steamID } });
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerAvatar))]
        [HarmonyPatch(nameof(PlayerAvatar.PlayerDeathRPC))]
        static void PlayerAvatarPlayerDeathRPCPostfix(PlayerAvatar __instance, int enemyIndex)
        {
            string presidentId = MutatorManager.Instance.Metadata[PresidentId];

            // If we are the president, do nothing. Else, die.
            if (__instance.steamID == presidentId && __instance.steamID != PlayerAvatar.instance.steamID)
            {
                _presidentAlive = false;
                PlayerAvatar? presidentAvatar = SemiFunc.PlayerGetFromSteamID(presidentId);
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
                PlayerAvatar playerAvatar = SemiFunc.PlayerGetFromSteamID(MutatorManager.Instance.Metadata[PresidentId]);
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
            string presidentId = MutatorManager.Instance.Metadata[PresidentId];
            if (__instance.playerAvatar.steamID != presidentId || presidentId == PlayerAvatar.instance.steamID) return;

            UpdatePresidentHealth(presidentId);
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
            string presidentId = MutatorManager.Instance.Metadata[PresidentId];

            if (presidentId != PlayerAvatar.instance.steamID)
            {
                MutatorsNetworkManager.Instance.Run(InitializePresidentHealthEnumerator(presidentId));
            }
        }

        private static void UpdatePresidentHealth(string presidentId)
        {
            TargetPlayerAnnouncingBehaviour targetPlayerAnnouncingBehaviour = TargetPlayerAnnouncingBehaviour.instance;
            if (!targetPlayerAnnouncingBehaviour) return;

            RepoMutators.Logger.LogDebug("Updating President Health");
            targetPlayerAnnouncingBehaviour.Text.text = BuildPresidentText(presidentId);
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

        private static void OnMetadataChanged(IDictionary<string, string> metadata)
        {
            RepoMutators.Logger.LogDebug("President metadata has changed");
            UpdatePresidentHealth(metadata[PresidentId]);

            MutatorManager.Instance.OnMetadataChanged -= OnMetadataChanged;
        }
    }
}
