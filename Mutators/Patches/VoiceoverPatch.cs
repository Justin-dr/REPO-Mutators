using HarmonyLib;
using Mutators.Extensions;
using Mutators.Managers;
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

        static void AfterPatchAll()
        {
            MutatorManager.Instance.OnMetadataChanged += OnMetadataChanged;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(LevelGenerator))]
        [HarmonyPatch(nameof(LevelGenerator.GenerateDone))]
        static void LevelGeneratorGenerateDone()
        {
            if (!SemiFunc.IsMasterClient()) return;

            IList<PlayerAvatar> playerAvatars = SemiFunc.PlayerGetAll();

            MutatorsNetworkManager.Instance.Run(WaitForVoiceChats(playerAvatars));
            //MutatorsNetworkManager.Instance.Run(CheckForVoice());
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(NetworkManager))]
        [HarmonyPatch(nameof(NetworkManager.OnPlayerLeftRoom))]
        static void NetworkManagerOnPlayerLeftRoomPrefix(Player otherPlayer)
        {
            RepoMutators.Logger.LogInfo("Player left triggered");
            PlayerAvatar leavingPlayer = SemiFunc.PlayerGetFromName(otherPlayer.NickName);
            string leavingPlayerHasVoiceOf = voiceOwnership[leavingPlayer.steamID];

            RepoMutators.Logger.LogInfo($"{SemiFunc.PlayerAvatarGetFromSteamID(leavingPlayerHasVoiceOf).playerName} returning own voice: {originalVoiceChats[leavingPlayerHasVoiceOf]}");
            ChangeVoices(leavingPlayerHasVoiceOf, originalVoiceChats[leavingPlayerHasVoiceOf]);
        }

        private static IEnumerator CheckForVoice()
        {
            while (true)
            {
                PlayerAvatar playerAvatar = PlayerAvatar.instance;
                if (playerAvatar.voiceChatFetched)
                {
                    if (playerAvatar.voiceChat)
                    {
                        RepoMutators.Logger.LogInfo($"I have a voice chat");
                    }
                    else
                    {
                        RepoMutators.Logger.LogInfo("I do not have a voice chat");
                    }
                }
                
                
                yield return new WaitForSeconds(1);
            }
        }

        private static IEnumerator WaitForVoiceChats(IList<PlayerAvatar> playerAvatars)
        {
            while (playerAvatars.Any(avatar => !avatar.voiceChatFetched))
            {
                yield return new WaitForSeconds(0.1f);
            }

            originalVoiceChats = playerAvatars.ToDictionary(a => a.steamID, a => a.voiceChat.photonView.ViewID);

            IDictionary<string, string> voiceOwnership = DerangeVoices(originalVoiceChats, out Dictionary<string, int> newAssignments);

            //foreach (PlayerAvatar playerAvatar in playerAvatars)
            //{
            //    int newVoiceChatId = newAssignments[playerAvatar.steamID];
            //    playerAvatar.photonView.RPC("UpdateMyPlayerVoiceChat", RpcTarget.AllBuffered, newVoiceChatId);
            //}

            IDictionary<string, object> metadata = new Dictionary<string, object>() {
                { "voices", newAssignments },
                { "voiceOwnership", voiceOwnership},
                { "originalVoices", originalVoiceChats}
            };
            MutatorsNetworkManager.Instance.SendMetadata(metadata);

            voiceOwnership.ForEach(kvp => RepoMutators.Logger.LogInfo($"Gave voice of {SemiFunc.PlayerAvatarGetFromSteamID(kvp.Key)?.playerName} to {SemiFunc.PlayerAvatarGetFromSteamID(kvp.Value)?.playerName}"));
            foreach (KeyValuePair<string, int> playerVoice in newAssignments)
            {
                ChangeVoices(playerVoice.Key, playerVoice.Value);
            }
        }

        // Returns the new mapping AND who owns whose voice
        public static Dictionary<string, string> DerangeVoices(IDictionary<string, int> originalMap, out Dictionary<string, int> newAssignments)
        {
            List<string> userIds = new List<string>(originalMap.Keys);
            List<int> voices = new List<int>(originalMap.Values);

            List<int> deranged = new List<int>(voices);
            int n = deranged.Count;

            // Perform derangement using Unity Random
            for (int i = 0; i < n - 1; i++)
            {
                int j = UnityEngine.Random.Range(i + 1, n);
                (deranged[i], deranged[j]) = (deranged[j], deranged[i]);
            }

            // Last element check
            if (EqualityComparer<object>.Default.Equals(deranged[n - 1], voices[n - 1]))
            {
                int swapWith = UnityEngine.Random.Range(0, n - 1);
                (deranged[n - 1], deranged[swapWith]) = (deranged[swapWith], deranged[n - 1]);
            }

            // Build final maps
            newAssignments = new Dictionary<string, int>();
            Dictionary<string, string> voiceOwnership = new Dictionary<string, string>();

            for (int i = 0; i < n; i++)
            {
                string userId = userIds[i];
                int newVoice = deranged[i];
                newAssignments[userId] = newVoice;

                // Reverse-lookup: who had this voice originally?
                int originalOwnerIndex = voices.IndexOf(newVoice);
                string originalOwnerId = userIds[originalOwnerIndex];

                voiceOwnership[userId] = originalOwnerId;
            }

            return voiceOwnership;
        }

        private static void OnMetadataChanged(IDictionary<string, object> metadata)
        {
            originalVoiceChats = metadata.Get<IDictionary<string, int>>("originalVoices");
            voiceOwnership = metadata.Get<IDictionary<string, string>>("voiceOwnership");

            IDictionary<string, int> voices = metadata.Get<IDictionary<string, int>>("voices");
            foreach (KeyValuePair<string, int> playerVoice in voices)
            {
                ChangeVoices(playerVoice.Key, playerVoice.Value);
            }

            MutatorManager.Instance.OnMetadataChanged -= OnMetadataChanged;
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
                playerAvatar.voiceChat.ToggleLobby(_toggle: false);
            }
        }

        private static void BeforeUnpatchAll()
        {
            originalVoiceChats.Clear();
            voiceOwnership.Clear();
        }
    }
}
