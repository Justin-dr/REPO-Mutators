using HarmonyLib;
using System;
using Unity.VisualScripting;
using UnityEngine.Events;

namespace Mutators.Mutators.Patches
{
    internal class VolatileCargoPatch
    {
        private static ExplosionPreset explosionPreset = null!; 

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ValuableObject))]
        [HarmonyPatch(nameof(ValuableObject.DollarValueSetLogic))]
        static void ValuableObjectDollarValueSetLogicPostfix(ValuableObject __instance)
        {
            AddExplosion(__instance, __instance.dollarValueOriginal);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ValuableObject))]
        [HarmonyPatch(nameof(ValuableObject.DollarValueSetRPC))]
        static void ValuableObjectDollarValueSetRPCPostfix(ValuableObject __instance, float value)
        {
            AddExplosion(__instance, value);
        }

        private static void Explode(ValuableObject valuableObject, ParticleScriptExplosion particleScriptExplosion, float size, int damage)
        {
            particleScriptExplosion.Spawn(valuableObject.transform.position, size, damage, damage);
        }

        private static void AddExplosion(ValuableObject __instance, float value)
        {
            if (explosionPreset == null)
            {
                explosionPreset = UnityEngine.Object.Instantiate(AssetStore.Preset);
            }

            ParticleScriptExplosion particleScriptExplosion = __instance.GetOrAddComponent<ParticleScriptExplosion>();

            if (particleScriptExplosion.explosionPreset == null)
            {
                particleScriptExplosion.explosionPreset = explosionPreset;
            }

            int damage = (int)Math.Ceiling(Math.Max(25f, value / 250));
            float size = Math.Clamp(value / 12000, 0.5f, 3f);

            PhysGrabObjectImpactDetector impactDetector = __instance.GetComponent<PhysGrabObjectImpactDetector>();

            if (impactDetector != null)
            {
                impactDetector.onDestroy.AddListener(new UnityAction(() => Explode(__instance, particleScriptExplosion, size, damage)));
            }
            else
            {
                RepoMutators.Logger.LogWarning($"[Volatile Cargo] unable to find impactDetector on {__instance.transform.name}");
            }
        }
    }
}
