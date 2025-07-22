using System.Linq;
using REPOLib.Modules;
using UnityEngine;

namespace Mutators.Assets.Loaders
{
    internal class ExplosionAssetLoader : BaseGameAssetLoader
    {
        private const string Source = "Enemy - Bang";
        
        internal override void Load()
        {
            if (!TryLoadExplosionPreset())
            {
                RepoMutators.Logger.LogWarning($"Mutators {Mutators.Mutators.HandleWithCareName} and {Mutators.Mutators.OutWithABangName} will be excluded from selection.");
                return;
            }
            RepoMutators.Logger.LogDebug($"Registered ExplosionPreset ScriptableObject from source {Source}.");
        }

        private static bool TryLoadExplosionPreset()
        {
            EnemySetup? enemySetup = Enemies.AllEnemies.FirstOrDefault(setup => setup.name == Source);

            if (enemySetup == null || !enemySetup )
            {
                RepoMutators.Logger.LogWarning($"Unable to register ExplosionPreset: could not find source {Source}.");
                return false;
            }

            GameObject? bang = enemySetup.spawnObjects.FirstOrDefault(so => so.PrefabName == Source)?.Prefab;

            if (bang == null || !bang)
            {
                RepoMutators.Logger.LogWarning($"Unable to register ExplosionPreset: could not find source spawn object {Source}.");
                return false;
            }

            ExplosionPreset? explosionPreset = bang.GetComponentInChildren<ParticleScriptExplosion>(true)?.explosionPreset;

            if (explosionPreset == null || !explosionPreset)
            {
                RepoMutators.Logger.LogWarning($"Unable to register ExplosionPreset: could not find ExplosionPreset on {Source}.");
                return false;
            }
            
            AssetStore.Preset = explosionPreset;
            return true;
        }
    }
}