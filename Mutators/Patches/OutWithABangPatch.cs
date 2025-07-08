using HarmonyLib;
using Mutators.Extensions;
using Mutators.Managers;
using Mutators.Settings.Specific;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.Events;

namespace Mutators.Mutators.Patches
{
    [HarmonyPatch(typeof(EnemyHealth))]
    internal class OutWithABangPatch
    {
        private static ExplosionPreset explosionPreset = null!;

        private static readonly float ExplosionSizeFallback = 3f;
        private static readonly int ExplosionDamageFallback = 200;
        private static readonly IDictionary<EnemyParent.Difficulty, (float size, int damage)> SizeDamage = new Dictionary<EnemyParent.Difficulty, (float, int)>();

        static void AfterPatchAll()
        {
            float tier1Radius = MutatorManager.Instance.Metadata.Get<float>(OutWithABangMutatorSettings.Tier1Radius);
            int tier1Damage = MutatorManager.Instance.Metadata.Get<int>(OutWithABangMutatorSettings.Tier1Damage);

            SizeDamage.Add(EnemyParent.Difficulty.Difficulty1, (tier1Radius, tier1Damage));

            float tier2Radius = MutatorManager.Instance.Metadata.Get<float>(OutWithABangMutatorSettings.Tier2Radius);
            int tier2Damage = MutatorManager.Instance.Metadata.Get<int>(OutWithABangMutatorSettings.Tier2Damage);

            SizeDamage.Add(EnemyParent.Difficulty.Difficulty2, (tier2Radius, tier2Damage));

            float tier3Radius = MutatorManager.Instance.Metadata.Get<float>(OutWithABangMutatorSettings.Tier3Radius);
            int tier3Damage = MutatorManager.Instance.Metadata.Get<int>(OutWithABangMutatorSettings.Tier3Damage);

            SizeDamage.Add(EnemyParent.Difficulty.Difficulty3, (tier3Radius, tier3Damage));
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(EnemyHealth.Awake))]
        static void EnemeyHealthAwakePostfix(EnemyHealth __instance)
        {
            if (explosionPreset == null)
            {
                explosionPreset = UnityEngine.Object.Instantiate(AssetStore.Preset);
            }

            EnemyParent enemyParent = __instance.enemy.EnemyParent;
            if (enemyParent.enemyName != "Banger")
            {
                ParticleScriptExplosion particleScriptExplosion = __instance.AddComponent<ParticleScriptExplosion>();
                particleScriptExplosion.explosionPreset = explosionPreset;


                float explosionSize = ExplosionSizeFallback;
                int explosionDamage = ExplosionDamageFallback;
                if (SizeDamage.TryGetValue(enemyParent.difficulty, out (float size, int damage) value))
                {
                    explosionSize = value.size;
                    explosionDamage = value.damage;
                }

                __instance.onDeath.AddListener(new UnityAction(() => Explode(__instance.enemy, particleScriptExplosion, explosionSize, explosionDamage)));
            }
        }

        private static void Explode(Enemy enemy, ParticleScriptExplosion particleScriptExplosion, float size, int damage)
        {
            particleScriptExplosion.Spawn(enemy.CenterTransform.position, size, damage, damage);
        }

        static void BeforeUnpatchAll()
        {
            SizeDamage.Clear();
        }
    }
}
