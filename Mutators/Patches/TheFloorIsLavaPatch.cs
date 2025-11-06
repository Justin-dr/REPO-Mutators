using HarmonyLib;
using Mutators.Extensions;
using Mutators.Managers;
using Mutators.Mutators.Behaviours;
using Mutators.Mutators.Behaviours.UI;
using Mutators.Network;
using Mutators.Settings;
using Mutators.Utility;
using Sirenix.Utilities;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Mutators.Mutators.Patches
{
    internal class TheFloorIsLavaPatch
    {
        internal const string Damage = "damage";
        internal const string UsePercentageDamage = "usePercentageDamage";
        internal const string ExtraDescription = "extraDescription";
        internal const string ImmunePlayers = "immunePlayers";
        internal const string RevivalImmunityDuration = "reviveImmunityDuration";

        internal static readonly System.Collections.Generic.ISet<PlayerAvatar> immunePlayers = new HashSet<PlayerAvatar>();
        private static float reviveImmunityDuration = MutatorSettings.TheFloorIsLava.ReviveImmunityDuration;
        private static int damage = MutatorSettings.TheFloorIsLava.DamagePerTick;

        private static bool initDone = false;

        static void OnMetadataChanged(IDictionary<string, object> metadata)
        {
            reviveImmunityDuration = metadata.Get<float>(RevivalImmunityDuration);
            damage = metadata.Get<int>(Damage);

            RepoMutators.Logger.LogInfo($"[The Floor Is Lava] Damage: {damage} - State {MutatorManager.Instance.GameState}");

            if (!initDone && MutatorManager.Instance.GameState == Enums.MutatorsGameState.LevelGenerated)
            {
                RepoMutators.Logger.LogInfo($"[The Floor Is Lava] Damage: {damage}");
                HandleImmuneLogic(
                    metadata.GetAsList<string>(ImmunePlayers) ?? [],
                    metadata.Get<string>(ExtraDescription),
                    damage,
                    metadata.Get<bool>(UsePercentageDamage)
                );
            }
        }

        [HarmonyPrefix]
        [HarmonyPriority(Priority.High + 1)]
        [HarmonyPatch(typeof(EnemyDirector))]
        [HarmonyPatch(nameof(EnemyDirector.GetEnemy))]
        static void EnemyDirectorGetEnemyPrefix(ref List<EnemySetup> ___enemyList, int ___enemyListIndex)
        {
            if (SemiFunc.IsMasterClientOrSingleplayer() && MutatorSettings.TheFloorIsLava.DisableEnemies)
            {
                ___enemyList.Clear();

                var emptyEnemySetup = ScriptableObject.CreateInstance<EnemySetup>();
                emptyEnemySetup.spawnObjects = new List<PrefabRef>();
                ___enemyList.Add(emptyEnemySetup);

                while (___enemyList.Count < ___enemyListIndex + 1)
                {
                    ___enemyList.Add(emptyEnemySetup);
                }
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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerAvatar))]
        [HarmonyPatch(nameof(PlayerAvatar.ReviveRPC))]
        static void PlayerAvatarReviveRPCPostfix(PlayerAvatar __instance)
        {
            if (__instance == PlayerAvatar.instance && reviveImmunityDuration > 0)
            {
                TheFloorIsLavaBehaviour theFloorIsLavaBehaviour = __instance.GetComponent<TheFloorIsLavaBehaviour>();
                if (theFloorIsLavaBehaviour != null)
                {
                    theFloorIsLavaBehaviour.immunityTimer = reviveImmunityDuration;
                }
                else
                {
                    RepoMutators.Logger.LogWarning("No TheFloorIsLavaBehaviour found on local player!");
                }
            }
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

        private static void SendMetadata()
        {
            IDictionary<string, object> metadata = new Dictionary<string, object>();

            if (immunePlayers.Count > 0)
            {
                metadata.Add(ImmunePlayers, immunePlayers.Select(player => player.steamID).ToList());

                string extraDescription = $"{JoinWithAnd(immunePlayers.Select(p => p.playerName).ToList())} {(immunePlayers.Count == 1 ? "is" : "are")} immune to lava damage!";
                metadata.Add(ExtraDescription, extraDescription);

                MutatorsNetworkManager.Instance.SendMetadata(metadata.WithMutator(MutatorSettings.TheFloorIsLava.MutatorName));
            }

            ApplyImmunity(MutatorSettings.TheFloorIsLava.DamagePerTick, MutatorSettings.TheFloorIsLava.UsePercentageDamage);
        }

        static void AfterUnpatchAll()
        {
            immunePlayers.Clear();
            initDone = false;
            damage = MutatorSettings.TheFloorIsLava.DamagePerTick;
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
