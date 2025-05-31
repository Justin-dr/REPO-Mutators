using HarmonyLib;
using Mutators.Mutators.Behaviours;
using Mutators.Mutators.Behaviours.Custom;
using Mutators.Settings;
using UnityEngine;

namespace Mutators.Mutators.Patches
{
    internal class FiringMyLaserPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerAvatar))]
        [HarmonyPatch(nameof(PlayerAvatar.Start))]
        static void PlayerAvatarStartPostfix(PlayerAvatar __instance)
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

            GameObject? laser = REPOLib.Modules.NetworkPrefabs.SpawnNetworkPrefab(
                $"{MyPluginInfo.PLUGIN_GUID}/FiringMyLaser", Vector3.zero, Quaternion.identity,
                data: [__instance.steamID, MutatorSettings.FiringMyLaser.LaserActionCooldown]
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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerHealth))]
        [HarmonyPatch(nameof(PlayerHealth.Hurt))]
        static void PlayerHealthHurtPostfix(PlayerHealth __instance, int damage)
        {

            if (damage < 1) return;

            LaserFiringBehaviour laserFiringBehaviour = __instance.playerAvatar.GetComponentInChildren<LaserFiringBehaviour>();

            if (laserFiringBehaviour && !laserFiringBehaviour.IsActive())
            {
                laserFiringBehaviour.FireLaser(2.5f);
            }
        }
    }
}
