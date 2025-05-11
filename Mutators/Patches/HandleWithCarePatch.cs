using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Mutators.Mutators.Patches
{
    internal class HandleWithCarePatch
    {
        private static bool _haulGoalReset = false;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ValuableObject))]
        [HarmonyPatch(nameof(ValuableObject.DollarValueSetLogic))]
        static void LevelGeneratorGenerateDonePostfix(ValuableObject __instance)
        {
            _haulGoalReset = false;
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                __instance.dollarValueCurrent *= Settings.HandleWithCareValueMultiplier.Value;
                __instance.dollarValueOriginal *= Settings.HandleWithCareValueMultiplier.Value;
            }
            
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(RoundDirector))]
        [HarmonyPatch(nameof(RoundDirector.StartRoundLogic))]
        static void RoundDirectorStartRoundLogicPrefix(ref int value)
        {
            if (!_haulGoalReset && SemiFunc.IsMasterClientOrSingleplayer())
            {
                float newValue = value / Settings.HandleWithCareValueMultiplier.Value;
                value = (int)newValue;
                _haulGoalReset = true;
            }
            
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PhysGrabObjectImpactDetector))]
        [HarmonyPatch(nameof(PhysGrabObjectImpactDetector.BreakRPC))]
        static void PhysGrabObjectImpactDetectorBreakRPCPrefix(PhysGrabObjectImpactDetector __instance, ref float valueLost, bool _loseValue)
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

            // Clients see incorrect amount being deducted, figure out why
            if (__instance.isValuable && _loseValue)
            {
                valueLost = __instance.valuableObject.dollarValueCurrent;
            }
        }
    }
}
