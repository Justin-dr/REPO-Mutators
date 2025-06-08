using HarmonyLib;
using Mutators.Extensions;
using Mutators.Managers;
using Mutators.Mutators.Behaviours;
using Mutators.Mutators.Behaviours.UI;
using Mutators.Network;
using Mutators.Settings;
using Mutators.Utility;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

namespace Mutators.Mutators.Patches
{
    internal class TheFloorIsLavaPatch
    {
        private const string Damage = "damage";
        private const string UsePercentageDamage = "usePercentageDamage";
        private const string ExtraDescription = "extraDescription";
        private const string ImmunePlayers = "immunePlayers";
        internal static readonly System.Collections.Generic.ISet<PlayerAvatar> immunePlayers = new HashSet<PlayerAvatar>();
        private static bool initDone = false;

        static void AfterPatchAll()
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer())
            {
                MutatorManager.Instance.OnMetadataChanged += OnMetaDataChanged;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(LevelGenerator))]
        [HarmonyPatch(nameof(LevelGenerator.GenerateDone))]
        static void LevelGeneratorGenerateDonePostfix()
        {
            if (SemiFunc.IsMultiplayer() && SemiFunc.IsMasterClient())
            {
                IList<PlayerAvatar> eligiblePlayers = SemiFunc.PlayerGetAll().ToList();
                if (MutatorSettings.TheFloorIsLava.ImmunePlayerCount > 0 && eligiblePlayers.Count > 1)
                {
                    int count = UnityEngine.Mathf.Min(MutatorSettings.TheFloorIsLava.ImmunePlayerCount, eligiblePlayers.Count);
                    for (int i = 0; i < count; i++)
                    {
                        PlayerAvatar chosenPlayer = eligiblePlayers[UnityEngine.Random.RandomRangeInt(0, eligiblePlayers.Count)];
                        eligiblePlayers.Remove(chosenPlayer);
                        immunePlayers.Add(chosenPlayer);
                    }
                }
                
                SendMetadata();
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerAvatar))]
        [HarmonyPatch(nameof(PlayerAvatar.Start))]
        static void PlayerAvatarAwakePostfix(PlayerAvatar __instance)
        {
            if (__instance == PlayerAvatar.instance)
            {
                __instance.AddComponent<TheFloorIsLavaBehaviour>();
            }
        }

        private static void OnMetaDataChanged(IDictionary<string, object> metadata)
        {
            HandleImmuneLogic(
                metadata.GetAsList<string>(ImmunePlayers),
                metadata.Get<string>(ExtraDescription),
                metadata.Get<int>(Damage),
                metadata.Get<bool>(UsePercentageDamage)
            );


            MutatorManager.Instance.OnMetadataChanged -= OnMetaDataChanged;
        }

        private static void HandleImmuneLogic(IList<string> immunePlayerList, string? extraDescription, int damagePerTick, bool usePercentageDamage)
        {
            immunePlayers.Clear();
            immunePlayers.AddRange(immunePlayerList.Select(ip => SemiFunc.PlayerAvatarGetFromSteamID(ip)).ToList());

            ApplyImmunity(damagePerTick, usePercentageDamage);
            HandleDescription(extraDescription);
        }

        private static void ApplyImmunity(int damagePerTick, bool usePercentageDamage)
        {
            TheFloorIsLavaBehaviour theFloorIsLavaBehaviour = PlayerAvatar.instance.GetComponent<TheFloorIsLavaBehaviour>();
            if (theFloorIsLavaBehaviour)
            {
                theFloorIsLavaBehaviour.Immune = immunePlayers.Contains(PlayerAvatar.instance);
                theFloorIsLavaBehaviour.damagePerTick = damagePerTick;
                theFloorIsLavaBehaviour.usePercentageDamage = usePercentageDamage;
            }
            else
            {
                RepoMutators.Logger.LogWarning("No TheFloorIsLavaBehaviour found on local player!");
            }
        }

        private static void HandleDescription(string? extraDescription)
        {
            if (initDone)
            {
                if (extraDescription != null)
                {
                    MutatorDescriptionAnnouncingBehaviour.Instance.Text.text = MutatorSettings.TheFloorIsLava.MutatorDescription + $"\n{extraDescription}";
                }
            }
            else
            {
                initDone = true;
                if (extraDescription != null)
                {
                    MutatorsNetworkManager.Instance.Run(DescriptionUtils.LateUpdateDescription(extraDescription, DescriptionUtils.DescriptionReplacementType.APPEND));
                }
            }
        }

        private static IDictionary<string, object> BuildMeta()
        {
            IDictionary<string, object> metadata = new Dictionary<string, object>()
            {
                { Damage, MutatorSettings.TheFloorIsLava.DamagePerTick },
                { UsePercentageDamage, MutatorSettings.TheFloorIsLava.UsePercentageDamage }
            };

            if (immunePlayers.Count > 0)
            {
                metadata.Add(ImmunePlayers, immunePlayers.Select(player => player.steamID).ToList());

                string extraDescription = $"{JoinWithAnd(immunePlayers.Select(p => p.playerName).ToList())} {(immunePlayers.Count == 1 ? "is" : "are")} immune to lava damage!";
                metadata.Add(ExtraDescription, extraDescription);
            }

            return metadata;
        }

        private static void SendMetadata()
        {
            IDictionary<string, object> metadata = BuildMeta();

            MutatorsNetworkManager.Instance.SendMetadata(metadata);

            ApplyImmunity(MutatorSettings.TheFloorIsLava.DamagePerTick, MutatorSettings.TheFloorIsLava.UsePercentageDamage);
            HandleDescription(metadata.Get<string>(ExtraDescription));
        }

        static void AfterUnpatchAll()
        {
            immunePlayers.Clear();
            initDone = false;
            MutatorManager.Instance.OnMetadataChanged -= OnMetaDataChanged;
        }

        private static string JoinWithAnd(IList<string> items)
        {
            if (items == null || items.Count == 0)
                return "";

            if (items.Count == 1)
                return items[0];

            if (items.Count == 2)
                return $"{items[0]} and {items[1]}";

            return string.Join(", ", items.Take(items.Count - 1)) + " and " + items.Last();
        }
    }
}
