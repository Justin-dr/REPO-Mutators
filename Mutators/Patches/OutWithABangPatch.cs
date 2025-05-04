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
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

            if (explosionPreset == null)
            {
                explosionPreset = UnityEngine.Object.Instantiate(AssetStore.Preset);
            }

            RepoMutators.Logger.LogInfo($"Adding explosion to {__instance.enemy.name}");
            ParticleScriptExplosion particleScriptExplosion = __instance.AddComponent<ParticleScriptExplosion>();
            particleScriptExplosion.explosionPreset = explosionPreset;

            __instance.onDeath.AddListener(new UnityAction(() => Explode(__instance.enemy, particleScriptExplosion)));
        }

        private static void Explode(Enemy enemy, ParticleScriptExplosion particleScriptExplosion)
        {
            RepoMutators.Logger.LogInfo($"Exploding at position {enemy.CenterTransform.position.ToString() ?? "null"}");
            particleScriptExplosion.Spawn(enemy.CenterTransform.position, 0.5f, 100, 100);
        }
    }
}
