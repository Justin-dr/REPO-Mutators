using HarmonyLib;
using Mutators.Managers;
using Mutators.Network;
using System.Collections.Generic;
using UnityEngine;

namespace Mutators.Mutators.Patches
{
    internal class ProtectThePresidentPatch
    {
        private static bool _presidentAlive = true;
        private static bool _failureMessageSent = false;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(LevelGenerator))]
        [HarmonyPatch(nameof(LevelGenerator.GenerateDone))]
        static void GenerateDone()
        {
            _presidentAlive = true;
            _failureMessageSent = false;
            if (SemiFunc.IsMultiplayer() && SemiFunc.IsMasterClient())
            {
                List<PlayerAvatar> playerAvatars = SemiFunc.PlayerGetAll();
                PlayerAvatar president = playerAvatars[UnityEngine.Random.RandomRangeInt(0, playerAvatars.Count)];
                RepoMutators.Logger.LogDebug($"Picked {president.playerName} as the president!");

                MutatorsNetworkManager.Instance.SendMetadata(new Dictionary<string, string>() { { "presidentId", president.steamID } });
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerAvatar))]
        [HarmonyPatch(nameof(PlayerAvatar.PlayerDeathRPC))]
        static void PlayerAvatarPlayerDeathRPCPostfix(PlayerAvatar __instance, int enemyIndex)
        {
            string presidentId = MutatorManager.Instance.Metadata["presidentId"];

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
                PlayerAvatar playerAvatar = SemiFunc.PlayerGetFromSteamID(MutatorManager.Instance.Metadata["presidentId"]);
                message = $"We have failed {playerAvatar?.playerName ?? " the president"}";
                _failureMessageSent = true;
            }
        }
    }
}
