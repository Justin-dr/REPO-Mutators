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
using Unity.VisualScripting;
using UnityEngine;

namespace Mutators.Mutators.Patches
{
    internal class TheFloorIsLavaPatch
    {
        private const string ExtraDescriptionAnnouncementKey = "floor-is-lava:extra-description";
        
        private const string ExtraDescription = "extraDescription";
        private const string ImmunePlayers = "immunePlayers";
        
        internal const string Damage = "damage";
        internal const string UsePercentageDamage = "is-percentage-damage";
        internal const string RevivalImmunityDuration = "revive-immunity-duration";

        private static readonly System.Collections.Generic.ISet<PlayerAvatar> immunePlayers = new HashSet<PlayerAvatar>();
        
        private static float reviveImmunityDuration = MutatorSettings.TheFloorIsLava.ReviveImmunityDuration;
        private static int damage = MutatorSettings.TheFloorIsLava.DamagePerTick;
        private static bool usePercentageDamage = MutatorSettings.TheFloorIsLava.UsePercentageDamage;

        private static bool initDone;

        static void OnMetadataChanged(IDictionary<string, object> metadata)
        {
            reviveImmunityDuration = metadata.Get<float>(RevivalImmunityDuration);
            damage = metadata.Get<int>(Damage);
            usePercentageDamage = metadata.Get<bool>(UsePercentageDamage);

            RepoMutators.Logger.LogInfo($"[The Floor Is Lava] Damage: {damage} - State {MutatorManager.Instance.GameState}");
            
            if (MutatorManager.Instance.GameState is MutatorsGameState.LevelReady or MutatorsGameState.LevelGenerated)
            {
                RepoMutators.Logger.LogInfo($"[The Floor Is Lava] Damage: {damage}");
                HandleImmuneLogic(
                    metadata.GetAsList<string>(ImmunePlayers) ?? [],
                    metadata.Get<string>(ExtraDescription),
                    damage,
                    usePercentageDamage
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
                    int count = Mathf.Min(MutatorSettings.TheFloorIsLava.ImmunePlayerCount, eligiblePlayers.Count);
                    for (int i = 0; i < count; i++)
                    {
                        PlayerAvatar chosenPlayer = eligiblePlayers[Random.RandomRangeInt(0, eligiblePlayers.Count)];
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
                TheFloorIsLavaBehaviour theFloorIsLavaBehaviour = __instance.AddComponent<TheFloorIsLavaBehaviour>();
                theFloorIsLavaBehaviour.damagePerTick = damage;
                theFloorIsLavaBehaviour.usePercentageDamage = usePercentageDamage;
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
            AbstractMutatorSettings settings = MutatorSettings.TheFloorIsLava;


            if (MutatorAnnouncingBag.Instance.TryGetAnnouncement(settings.NamespacedName, out MutatorAnnouncement? announcement))
            {
                if (extraDescription == null)
                {
                    announcement.RemoveSegment(ExtraDescriptionAnnouncementKey);
                }
                else
                {
                    announcement.AddOrUpdateSegment(new MutatorAnnouncementDescriptionSegment(
                        ExtraDescriptionAnnouncementKey,
                        10,
                        $"\n{extraDescription}"
                    ));
                }
                return;
            }
            RepoMutators.Logger.LogWarning("[The Floor Is Lava] Tried to update announcement, but it was not found!");
        }

        private static void SendMetadata()
        {
            IDictionary<string, object> metadata = new Dictionary<string, object>();

            if (immunePlayers.Count > 0)
            {
                metadata.Add(ImmunePlayers, immunePlayers.Select(player => player.steamID).ToList());
                
                RepoMutators.Logger.LogInfo("Immune players: " + string.Join(", ", immunePlayers.Select(p => $"{p.playerName} ({p.steamID})")));

                string extraDescription = $"{JoinWithAnd(immunePlayers.Select(p => p.playerName).ToList())} {(immunePlayers.Count == 1 ? "is" : "are")} immune to lava damage!";
                metadata.Add(ExtraDescription, extraDescription);

                MutatorsNetworkManager.Instance.SendMetadata(
                    MutatorSettings.TheFloorIsLava.NamespacedName,
                    metadata.WithMutator(MutatorSettings.TheFloorIsLava.NamespacedName)
                );
            }
        }

        static void AfterUnpatchAll()
        {
            immunePlayers.Clear();
            initDone = false;
            damage = MutatorSettings.TheFloorIsLava.DamagePerTick;
            usePercentageDamage = MutatorSettings.TheFloorIsLava.UsePercentageDamage;
            reviveImmunityDuration = MutatorSettings.TheFloorIsLava.ReviveImmunityDuration;
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
