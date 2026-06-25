using System;
using System.Linq;
using Mutators.Mutators.Behaviours;
using Mutators.Mutators.Behaviours.HurtColliders;
using Photon.Pun;
using REPOLib.Modules;
using UnityEngine;
using static UnityEngine.Object;

namespace Mutators.Assets.Loaders
{
    internal class LaserAssetLoader : BaseGameAssetLoader
    {
        private const string Source = "Valuable Wizard Dumgolfs Staff";

        internal override void Load()
        {
            bool result = TryRegisterFiringMyLaserPrefab();
            
            if (!result)
            {
                RepoMutators.Logger.LogWarning($"Mutator {Mutators.Mutators.FiringMyLaserName} will be excluded from selection.");
            }
            
            AssetStore.IsLaserLoaded = result;
        }

        private static bool TryRegisterFiringMyLaserPrefab()
        {
            if (NetworkPrefabs.HasNetworkPrefab(AssetStore.FIRING_MY_LASER_PREFAB_ID))
            {
                return true;
            }

            GameObject? prefab = CreateFiringMyLaserPrefab();
            if (prefab == null || !prefab)
            {
                RepoMutators.Logger.LogWarning($"Unable to register FiringMyLaser: could not find source {Source}.");
                return false;
            }

            NetworkPrefabs.RegisterNetworkPrefab(AssetStore.FIRING_MY_LASER_PREFAB_ID, prefab);
            RepoMutators.Logger.LogDebug($"Registered FiringMyLaser network prefab from source {Source}.");
            return true;
        }

        private static GameObject? CreateFiringMyLaserPrefab()
        {
            SemiLaser? sourceLaser = FindLaserOnValuable();
            if (sourceLaser == null || !sourceLaser)
            {
                return null;
            }

            GameObject prefab = new("FiringMyLaser")
            {
                hideFlags = HideFlags.HideAndDontSave,
            };
            prefab.SetActive(false);

            GameObject semiLaser = Instantiate(sourceLaser.gameObject, prefab.transform, false);
            semiLaser.SetActive(true);
            ReplaceHurtColliders(prefab);

            prefab.AddComponent<PhotonView>();
            prefab.AddComponent<LaserFiringBehaviour>();

            return prefab;
        }

        private static SemiLaser? FindLaserOnValuable()
        {
            PrefabRef? prefabRef = Valuables.AllValuables.FirstOrDefault(v => string.Equals(v.PrefabName, Source, StringComparison.InvariantCultureIgnoreCase));

            GameObject? staffPrefab = prefabRef?.Prefab;

            return staffPrefab?.GetComponentInChildren<SemiLaser>(true);
        }

        private static void ReplaceHurtColliders(GameObject prefab)
        {
            foreach (HurtCollider hurtCollider in prefab.GetComponentsInChildren<HurtCollider>(true))
            {
                if (hurtCollider is PlayerIgnoringHurtCollider)
                {
                    continue;
                }

                PlayerIgnoringHurtCollider replacement = hurtCollider.gameObject.AddComponent<PlayerIgnoringHurtCollider>();
                replacement.playerKill = true;
                replacement.enemyDamage = 30;
                replacement.enemyKill = false;
                replacement.enemyStun = hurtCollider.enemyStun;
                replacement.enemyStunTime = hurtCollider.enemyStunTime;
                replacement.enemyHitForce = hurtCollider.enemyHitForce;
                replacement.enemyHitTorque = hurtCollider.enemyHitTorque;
                replacement.playerDamageCooldown = hurtCollider.playerDamageCooldown;
                replacement.physDamageCooldown = hurtCollider.physDamageCooldown;
                replacement.physImpact = hurtCollider.physImpact;
                replacement.physHitForce = hurtCollider.physHitForce;
                replacement.physHitTorque = hurtCollider.physHitTorque;
                replacement.hitSpread = hurtCollider.hitSpread;
                DestroyImmediate(hurtCollider);
            }
        }
    }
}