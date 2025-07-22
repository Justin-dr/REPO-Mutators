using System.Collections.Generic;
using HarmonyLib;
using Mutators.Announcements;
using Mutators.Assets;
using Mutators.Extensions;
using Mutators.Mutators.Behaviours;
using Mutators.Mutators.Behaviours.HurtColliders;
using Mutators.Settings;
using Mutators.Settings.Specific;
using REPOLib.Modules;
using UnityEngine;

namespace Mutators.Mutators.Patches
{
    internal class FiringMyLaserPatch
    {
        private static bool LaserBlocked = false;
        private static bool LaserOnHurt = true;

        static void OnMetadataChanged(IDictionary<string, object> metadata)
        {
            LaserOnHurt = metadata.Get<bool>(FiringMyLaserMutatorSettings.LaserOnHurtEnabledKey);
            bool laserOnAction = metadata.Get<bool>(FiringMyLaserMutatorSettings.LaserActionEnabledKey);
            
            FiringMyLaserMutatorSettings settings = MutatorSettings.FiringMyLaser;
            if (!MutatorAnnouncingBag.Instance.TryGetAnnouncement(settings.NamespacedName, out MutatorAnnouncement? announcement))
            {
                RepoMutators.Logger.LogError($"[{settings.MutatorName}] Unable to find announcement, could not update description.");
                return;
            }
            
            if (!laserOnAction && !LaserOnHurt)
            {
                announcement.UpdateBaseDescription("Laser module not installed...");
                return;
            }
            
            if (!laserOnAction)
            {
                announcement.UpdateBaseDescription(Mutators.FiringMyLaserDescription.Split("\n")[1]);
            } else if (!LaserOnHurt)
            {
                announcement.UpdateBaseDescription(Mutators.FiringMyLaserDescription.Split("\n")[0]);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerAvatar))]
        [HarmonyPatch(nameof(PlayerAvatar.Start))]
        static void PlayerAvatarStartPostfix(PlayerAvatar __instance)
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

            string prefabId = AssetStore.FIRING_MY_LASER_PREFAB_ID;
            if (!NetworkPrefabs.TryGetNetworkPrefabRef(prefabId, out PrefabRef? prefabRef))
            {
                RepoMutators.Logger.LogError("Unable to instantiate laser: Could not find PrefabRef with id " + prefabId);
                return;
            }

            GameObject? laser = NetworkPrefabs.SpawnNetworkPrefab(
                prefabRef, Vector3.zero, Quaternion.identity,
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
            if (!LaserOnHurt || damage < 1 || __instance.playerAvatar.deadSet || LaserBlocked) return;

            LaserFiringBehaviour laserFiringBehaviour = __instance.playerAvatar.GetComponentInChildren<LaserFiringBehaviour>();
            if (laserFiringBehaviour && !laserFiringBehaviour.IsActive() && !laserFiringBehaviour.IsReviveLockout())
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
        static void PlayerAvatarDeathPostfix(PlayerAvatar __instance)
        {
            LaserFiringBehaviour laserFiringBehaviour = __instance.GetComponentInChildren<LaserFiringBehaviour>(true);

            if (laserFiringBehaviour)
            {
                laserFiringBehaviour.StopLaser(true);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerAvatar))]
        [HarmonyPatch(nameof(PlayerAvatar.Revive))]
        static void PlayerAvatarReviveRPCPostfix(PlayerAvatar __instance)
        {
            LaserFiringBehaviour laserFiringBehaviour = __instance.GetComponentInChildren<LaserFiringBehaviour>(true);

            if (laserFiringBehaviour)
            {
                laserFiringBehaviour.ActivateReviveLockout();
            }
        }

        private static void BeforeUnpatchAll()
        {
            LaserBlocked = false;
            LaserOnHurt = true;
        }
    }
}
