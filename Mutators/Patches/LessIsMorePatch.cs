using HarmonyLib;
using Mutators.Extensions;
using Mutators.Mutators.Behaviours;
using Mutators.Settings;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Mutators.Mutators.Patches
{
    internal class LessIsMorePatch
    {
        private const string SurplusValuable = "Surplus Valuable";
        internal const string ValueGainMultiplier = "ValueGainMultiplier";

        private static float valueGainMultiplier = MutatorSettings.LessIsMore.ValueGainMultiplier;

        static void OnMetadataChanged(IDictionary<string, object> metadata)
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer())
            {
                valueGainMultiplier = metadata.Get<float>(ValueGainMultiplier);
            }
        }

        [HarmonyPostfix]
        [HarmonyPriority(Priority.HigherThanNormal)]
        [HarmonyPatch(typeof(ValuableObject))]
        [HarmonyPatch(nameof(ValuableObject.DollarValueSetLogic))]
        static void ValuableObjectDollarValueSetLogicPostfix(ValuableObject __instance)
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                if (__instance.gameObject.name.StartsWith(SurplusValuable))
                {
                    return;
                }

                float t = Mathf.InverseLerp(100f, 30f, __instance.durabilityPreset.fragility);
                float divisor = Mathf.Lerp(MutatorSettings.LessIsMore.StrongDivisionFactor, MutatorSettings.LessIsMore.WeakDivisionFactor, t);

                __instance.dollarValueCurrent /= divisor;
                __instance.dollarValueOriginal /= divisor;

                __instance.AddComponent<LessIsMoreBehaviour>();
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(RoundDirector))]
        [HarmonyPatch(nameof(RoundDirector.StartRoundLogic))]
        static void RoundDirectorStartRoundLogic(ref int value)
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                value = (int)(value * 1.1);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PhysGrabObjectImpactDetector))]
        [HarmonyPatch(nameof(PhysGrabObjectImpactDetector.BreakRPC))]
        static void PhysGrabObjectImpactDetectorBreakRPC(PhysGrabObjectImpactDetector __instance, ref float valueLost, bool _loseValue)
        {
            if (!_loseValue || !__instance.isValuable || __instance.isNotValuable) return;
            if (__instance.gameObject.name.StartsWith(SurplusValuable))
            {
                return;
            }

            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                __instance.GetComponent<LessIsMoreBehaviour>()?.SubtractValue(valueLost);
            }

            valueLost = -valueLost * valueGainMultiplier;
        }

        static void AfterUnpatchAll()
        {
            valueGainMultiplier = MutatorSettings.LessIsMore.ValueGainMultiplier;
        }
    }
}
