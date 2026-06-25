using System;
using HarmonyLib;
using Mutators.Assets;
using Mutators.Mutators.Behaviours.Markers;
using Unity.VisualScripting;
using Object = UnityEngine.Object;

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
            if (__instance.GetComponent<VolatileCargoMarkerBehaviour>() != null) return;
            AddExplosion(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ValuableObject))]
        [HarmonyPatch(nameof(ValuableObject.DollarValueSetRPC))]
        static void ValuableObjectDollarValueSetRPCPostfix(ValuableObject __instance, float value)
        {
            if (__instance.GetComponent<VolatileCargoMarkerBehaviour>() != null) return;
            AddExplosion(__instance);
        }

        private static void Explode(ValuableObject valuableObject, ParticleScriptExplosion particleScriptExplosion)
        {
            float maxValue = Math.Max(valuableObject.dollarValueCurrent, valuableObject.dollarValueOriginal);
            int damage = (int)Math.Ceiling(Math.Max(25f, maxValue / 250));
            float size = Math.Clamp(maxValue / 12000, 0.5f, 3f);
            
            particleScriptExplosion.Spawn(valuableObject.transform.position, size, damage, damage);
        }

        private static void AddExplosion(ValuableObject __instance)
        {
            if (explosionPreset == null)
            {
                explosionPreset = Object.Instantiate(AssetStore.Preset);
            }

            ParticleScriptExplosion particleScriptExplosion = __instance.GetOrAddComponent<ParticleScriptExplosion>();

            if (particleScriptExplosion.explosionPreset == null)
            {
                particleScriptExplosion.explosionPreset = explosionPreset;
            }

            PhysGrabObjectImpactDetector impactDetector = __instance.GetComponent<PhysGrabObjectImpactDetector>();

            if (impactDetector != null)
            {
                impactDetector.onDestroy.AddListener(() => Explode(__instance, particleScriptExplosion));
                __instance.AddComponent<VolatileCargoMarkerBehaviour>();
            }
            else
            {
                RepoMutators.Logger.LogWarning($"[Volatile Cargo] unable to find impactDetector on {__instance.transform.name}");
            }
        }
    }
}
