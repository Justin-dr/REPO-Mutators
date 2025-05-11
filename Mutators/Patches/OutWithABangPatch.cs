using HarmonyLib;
using Unity.VisualScripting;
using UnityEngine.Events;

namespace Mutators.Mutators.Patches
{
    [HarmonyPatch(typeof(EnemyHealth))]
    internal class OutWithABangPatch
    {
        private static ExplosionPreset explosionPreset = null!;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(EnemyHealth.Awake))]
        static void EnemeyHealthAwakePostfix(EnemyHealth __instance)
        {
            if (explosionPreset == null)
            {
                explosionPreset = UnityEngine.Object.Instantiate(AssetStore.Preset);
            }

            if (__instance.enemy.EnemyParent.enemyName != "Banger")
            {
                ParticleScriptExplosion particleScriptExplosion = __instance.AddComponent<ParticleScriptExplosion>();
                particleScriptExplosion.explosionPreset = explosionPreset;

                __instance.onDeath.AddListener(new UnityAction(() => Explode(__instance.enemy, particleScriptExplosion)));
            }
        }

        private static void Explode(Enemy enemy, ParticleScriptExplosion particleScriptExplosion)
        {
            particleScriptExplosion.Spawn(enemy.CenterTransform.position, 0.5f, 25, 25);
        }
    }
}
