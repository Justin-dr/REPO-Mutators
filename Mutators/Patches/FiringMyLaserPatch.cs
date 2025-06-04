using HarmonyLib;
using Mutators.Mutators.Behaviours;
using Mutators.Mutators.Behaviours.Custom;
using Mutators.Settings;
using UnityEngine;

namespace Mutators.Mutators.Patches
{
    internal class FiringMyLaserPatch
    {
        private static bool LaserBlocked = false;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerAvatar))]
        [HarmonyPatch(nameof(PlayerAvatar.Start))]
        static void PlayerAvatarStartPostfix(PlayerAvatar __instance)
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

            GameObject? laser = REPOLib.Modules.NetworkPrefabs.SpawnNetworkPrefab(
                $"{MyPluginInfo.PLUGIN_GUID}/FiringMyLaser", Vector3.zero, Quaternion.identity,
                data: [__instance.steamID, MutatorSettings.FiringMyLaser.LaserActionCooldown, MutatorSettings.FiringMyLaser.LaserActionEnemyDamage]
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

        internal static void Reset()
        {
            LaserBlocked = false;
        }
    }
}
