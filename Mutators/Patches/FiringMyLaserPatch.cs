using HarmonyLib;
using Mutators.Extensions;
using Mutators.Mutators.Behaviours;
using Mutators.Mutators.Behaviours.Custom;
using Mutators.Network;
using Mutators.Settings;
using Mutators.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace Mutators.Mutators.Patches
{
    internal class FiringMyLaserPatch
    {
        private const string LaserActionEnabled = "laserActionEnabled";
        private static bool LaserBlocked = false;

        static void OnMetadataChanged(IDictionary<string, object> metadata)
        {
            if (!metadata.Get<bool>(LaserActionEnabled))
            {
                MutatorsNetworkManager.Instance.Run(DescriptionUtils.LateUpdateDescription(Mutators.FiringMyLaserDescription.Split("\n")[1]));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(LevelGenerator))]
        [HarmonyPatch(nameof(LevelGenerator.GenerateDone))]
        static void LevelGeneratorGenerateDonePostfix()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                IDictionary<string, object> metadata = new Dictionary<string, object>()
                {
                    { LaserActionEnabled, MutatorSettings.FiringMyLaser.LaserActionEnabled }
                };

                MutatorsNetworkManager.Instance.SendMetadata(metadata);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerAvatar))]
        [HarmonyPatch(nameof(PlayerAvatar.Start))]
        static void PlayerAvatarStartPostfix(PlayerAvatar __instance)
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

            GameObject? laser = REPOLib.Modules.NetworkPrefabs.SpawnNetworkPrefab(
                $"{MyPluginInfo.PLUGIN_GUID}/FiringMyLaser", Vector3.zero, Quaternion.identity,
                data: [__instance.steamID, MutatorSettings.FiringMyLaser.LaserActionCooldown, MutatorSettings.FiringMyLaser.LaserActionEnemyDamage, MutatorSettings.FiringMyLaser.LaserActionEnabled]
            );

            if (!laser || laser == null)
            {
                RepoMutators.Logger.LogWarning($"Failed to create laser for {__instance.playerName}");
            }
            else if (!SemiFunc.IsMultiplayer())
            {
                laser.transform.SetParent(__instance.transform, false);
                LaserFiringBehaviour laserFiringBehaviour = laser.GetComponentInChildren<LaserFiringBehaviour>();
                if (laserFiringBehaviour)
                {
                    laser.SetActive(true);
                    laserFiringBehaviour.laserCooldown = MutatorSettings.FiringMyLaser.LaserActionCooldown;
                    laserFiringBehaviour.laserCooldownTimer = MutatorSettings.FiringMyLaser.LaserActionCooldown;
                    laserFiringBehaviour.manualActionEnabled = MutatorSettings.FiringMyLaser.LaserActionEnabled;
                    laser.GetComponentInChildren<PlayerIgnoringHurtCollider>(true).enemyDamage = MutatorSettings.FiringMyLaser.LaserActionEnemyDamage;
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HurtCollider))]
        [HarmonyPatch(nameof(HurtCollider.PlayerHurt))]
        static bool HurtColliderPlayerHurt(HurtCollider __instance, PlayerAvatar _player)
        {
            if (__instance is PlayerIgnoringHurtCollider playerIgnoring)
            {
                if (playerIgnoring.ignoredPlayers.Contains(_player))
                {
                    return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerHealth))]
        [HarmonyPatch(nameof(PlayerHealth.HurtOtherRPC))]
        static void PlayerHealthHurtOtherRPCPrefix(PlayerHealth __instance, int damage, Vector3 hurtPosition, int enemyIndex)
        {
            if (__instance.playerAvatar != PlayerAvatar.instance || enemyIndex != -1 || hurtPosition != Vector3.zero) return;
            LaserBlocked = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerHealth))]
        [HarmonyPatch(nameof(PlayerHealth.Hurt))]
        static void PlayerHealthHurtPostfix(PlayerHealth __instance, int damage)
        {
            if (damage < 1 || __instance.playerAvatar.deadSet || LaserBlocked) return;

            LaserFiringBehaviour laserFiringBehaviour = __instance.playerAvatar.GetComponentInChildren<LaserFiringBehaviour>();
            if (laserFiringBehaviour && !laserFiringBehaviour.IsActive())
            {
                laserFiringBehaviour.FireLaser(2.5f);
            }
        }

        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last + 1)]
        [HarmonyPatch(typeof(PlayerHealth))]
        [HarmonyPatch(nameof(PlayerHealth.Hurt))]
        static void PlayerHealthHurtLaserResetPostfix()
        {
            LaserBlocked = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerAvatar))]
        [HarmonyPatch(nameof(PlayerAvatar.PlayerDeath))]
        static void PlayerAvatarReviveRPCPostfix(PlayerAvatar __instance)
        {
            LaserFiringBehaviour laserFiringBehaviour = __instance.GetComponentInChildren<LaserFiringBehaviour>(true);

            if (laserFiringBehaviour)
            {
                laserFiringBehaviour.StopLaser(true);
            }
        }

        private static void BeforeUnpatchAll()
        {
            LaserBlocked = false;
        }
    }
}
