using HarmonyLib;
using Mutators.Settings;

namespace Mutators.Mutators.Patches
{
    internal class HandleWithCarePatch
    {
        private const string SurplusValuable = "Surplus Valuable";
        private static bool _haulGoalReset = false;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ValuableObject))]
        [HarmonyPatch(nameof(ValuableObject.DollarValueSetLogic))]
        static void ValuableObjectDollarValueSetLogicPostfix(ValuableObject __instance)
        {
            _haulGoalReset = false;
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                if (!MutatorSettings.HandleWithCare.MultiplySurplusValue && __instance.gameObject.name.StartsWith(SurplusValuable)) 
                {
                    return;
                }
                __instance.dollarValueCurrent *= MutatorSettings.HandleWithCare.ValueMultiplier;
                __instance.dollarValueOriginal *= MutatorSettings.HandleWithCare.ValueMultiplier;
            }
            
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(RoundDirector))]
        [HarmonyPatch(nameof(RoundDirector.StartRoundLogic))]
        static void RoundDirectorStartRoundLogicPrefix(ref int value)
        {
            if (!_haulGoalReset && SemiFunc.IsMasterClientOrSingleplayer())
            {
                float newValue = value / MutatorSettings.HandleWithCare.ValueMultiplier;
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
            if (!MutatorSettings.HandleWithCare.InstantlyDestroySurplus && __instance.name.StartsWith(SurplusValuable)) return;

            // Clients see incorrect amount being deducted, figure out why
            if (__instance.isValuable && _loseValue)
            {
                valueLost = __instance.valuableObject.dollarValueCurrent;
            }
        }
    }
}
