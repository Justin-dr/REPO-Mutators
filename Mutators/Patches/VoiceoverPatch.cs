using HarmonyLib;
using Mutators.Extensions;
using Mutators.Network;
using Photon.Pun;
using Photon.Realtime;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mutators.Mutators.Patches
{
    internal class VoiceoverPatch
    {
        private static IDictionary<string, int> originalVoiceChats = new Dictionary<string, int>();
        private static IDictionary<string, string> voiceOwnership = new Dictionary<string, string>();

        private static void OnMetadataChanged(IDictionary<string, object> metadata)
        {
            IDictionary<string, int> originals = metadata.Get<IDictionary<string, int>>("originalVoices");
            IDictionary<string, string> ownership = metadata.Get<IDictionary<string, string>>("voiceOwnership");
            if (originalVoiceChats.Count == 0 && originals?.Count > 0 && ownership.Count > 0)
            {
                originalVoiceChats = originals;
                voiceOwnership = ownership;

                IDictionary<string, int> voices = metadata.Get<IDictionary<string, int>>("voices");
                foreach (KeyValuePair<string, int> playerVoice in voices)
                {
                    ChangeVoices(playerVoice.Key, playerVoice.Value);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(LevelGenerator))]
        [HarmonyPatch(nameof(LevelGenerator.GenerateDone))]
        static void LevelGeneratorGenerateDone()
        {
            if (!SemiFunc.IsMasterClient()) return;

            IList<PlayerAvatar> playerAvatars = SemiFunc.PlayerGetAll();

            MutatorsNetworkManager.Instance.Run(WaitForVoiceChats(playerAvatars));
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(NetworkManager))]
        [HarmonyPatch(nameof(NetworkManager.OnPlayerLeftRoom))]
        static void NetworkManagerOnPlayerLeftRoomPrefix(Player otherPlayer)
        {
            // Identify the leaving player
            string? leavingPlayerId = SemiFunc.PlayerGetFromName(otherPlayer.NickName)?.steamID;
            if (leavingPlayerId == null || !originalVoiceChats.ContainsKey(leavingPlayerId)) return;

            // Who’s voice the leaving player was holding
            string originalOwnerOfLeavingVoice = voiceOwnership[leavingPlayerId];

            // Who was holding the leaving player’s voice
            string recipientOfLeavingVoice = voiceOwnership.First(kvp => kvp.Value == leavingPlayerId).Key;

            // Restore the leaving player back to their own original view
            ChangeVoices(leavingPlayerId, originalVoiceChats[leavingPlayerId]);

            // Give the recipient the view that had been held by the leaver
            ChangeVoices(recipientOfLeavingVoice, originalVoiceChats[originalOwnerOfLeavingVoice]);

            // Update tracking for the recipient
            voiceOwnership[recipientOfLeavingVoice] = originalOwnerOfLeavingVoice;

            // Remove the leaving player from all maps
            originalVoiceChats.Remove(leavingPlayerId);
            voiceOwnership.Remove(leavingPlayerId);
        }

        private static IEnumerator WaitForVoiceChats(IList<PlayerAvatar> playerAvatars)
        {
            while (playerAvatars.Any(avatar => !avatar.voiceChatFetched))
            {
                yield return new WaitForSeconds(0.1f);
            }

            originalVoiceChats = playerAvatars.ToDictionary(a => a.steamID, a => a.voiceChat.photonView.ViewID);
            voiceOwnership = DerangeVoices(originalVoiceChats, out Dictionary<string, int> newAssignments);

            IDictionary<string, object> metadata = new Dictionary<string, object>() {
                { "voices", newAssignments },
                { "voiceOwnership", voiceOwnership},
                { "originalVoices", originalVoiceChats}
            };
            MutatorsNetworkManager.Instance.SendMetadata(metadata);

            voiceOwnership.ForEach(kvp => RepoMutators.Logger.LogDebug($"Gave voice of {SemiFunc.PlayerAvatarGetFromSteamID(kvp.Value)?.playerName} to {SemiFunc.PlayerAvatarGetFromSteamID(kvp.Key)?.playerName}"));
            foreach (KeyValuePair<string, int> playerVoice in newAssignments)
            {
                ChangeVoices(playerVoice.Key, playerVoice.Value);
            }

            // MutatorsNetworkManager.Instance.Run(Debug());
        }

        public static Dictionary<string, string> DerangeVoices(IDictionary<string, int> originalMap, out Dictionary<string, int> newAssignments)
        {
            var userIds = new List<string>(originalMap.Keys);
            var voices = new List<int>(originalMap.Values);

            int n = userIds.Count;
            var deranged = new List<int>(voices);

            // Fisher–Yates Derangement
            for (int i = 0; i < n - 1; i++)
            {
                int j = Random.Range(i + 1, n);
                (deranged[i], deranged[j]) = (deranged[j], deranged[i]);
            }

            if (deranged[n - 1] == voices[n - 1])
            {
                int swapWith = Random.Range(0, n - 1);
                (deranged[n - 1], deranged[swapWith]) = (deranged[swapWith], deranged[n - 1]);
            }

            newAssignments = new Dictionary<string, int>();
            var whoHasWhoseVoice = new Dictionary<string, string>();

            // Build originalVoiceOwner map: int voiceId → userId
            var voiceToOriginalUser = new Dictionary<int, string>();
            for (int i = 0; i < n; i++)
            {
                voiceToOriginalUser[voices[i]] = userIds[i];
            }

            for (int i = 0; i < n; i++)
            {
                string userId = userIds[i];
                int newVoice = deranged[i];

                newAssignments[userId] = newVoice;

                string originalOwner = voiceToOriginalUser[newVoice];
                whoHasWhoseVoice[userId] = originalOwner;
            }

            return whoHasWhoseVoice;
        }

        private static void ChangeVoices(string steamId, int photonViewID)
        {
            PlayerAvatar playerAvatar = SemiFunc.PlayerAvatarGetFromSteamID(steamId);
            playerAvatar.voiceChat = PhotonView.Find(photonViewID).GetComponent<PlayerVoiceChat>();
            playerAvatar.voiceChat.playerAvatar = playerAvatar;
            if (playerAvatar.voiceChat.TTSinstantiated)
            {
                playerAvatar.voiceChat.ttsVoice.playerAvatar = playerAvatar;
            }
            if (!SemiFunc.MenuLevel())
            {
                playerAvatar.voiceChat.ToggleMixer(_lobby: true);
            }
        }

        private static IEnumerator Debug()
        {
            PlayerVoiceChat playerVoiceChat = PlayerAvatar.instance.voiceChat;

            while (true)
            {
                RepoMutators.Logger.LogInfo($"ToggleMute: {playerVoiceChat.toggleMute} - Director: {DataDirector.instance.toggleMute}");
                RepoMutators.Logger.LogInfo($"Lobby: {playerVoiceChat.audioSource.outputAudioMixerGroup == playerVoiceChat.mixerMicrophoneSpectate} - Game: {playerVoiceChat.audioSource.outputAudioMixerGroup == playerVoiceChat.mixerMicrophoneSound}");
                RepoMutators.Logger.LogInfo($"Active and Enabled: {playerVoiceChat.isActiveAndEnabled}");
                yield return new WaitForSeconds(1);
            }
        }

        private static void BeforeUnpatchAll()
        {
            originalVoiceChats.Clear();
            voiceOwnership.Clear();
        }
    }
}
