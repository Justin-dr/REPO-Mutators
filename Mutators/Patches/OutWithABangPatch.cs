﻿using HarmonyLib;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.Events;

namespace Mutators.Mutators.Patches
{
    [HarmonyPatch(typeof(EnemyHealth))]
    internal class OutWithABangPatch
    {
        private static ExplosionPreset explosionPreset = null!;

        private static readonly float ExplosionSizeFallback = 1.2f;
        private static readonly int ExplosionDamageFallback = 100;
        private static readonly IDictionary<EnemyParent.Difficulty, (float size, int damage)> SizeDamage = InitDamageMap();

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

        private static IDictionary<EnemyParent.Difficulty, (float size, int damage)> InitDamageMap()
        {
            return new Dictionary<EnemyParent.Difficulty, (float size, int damage)>(){
                { EnemyParent.Difficulty.Difficulty1, (0.6f, 25) },
                { EnemyParent.Difficulty.Difficulty2, (0.9f, 50) },
                { EnemyParent.Difficulty.Difficulty3, (ExplosionSizeFallback, ExplosionDamageFallback) }
            };
        }
    }
}
