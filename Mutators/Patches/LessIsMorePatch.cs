using HarmonyLib;
using Mutators.Settings;

namespace Mutators.Mutators.Patches
{
    internal class LessIsMorePatch
    {
        private const string SurplusValuable = "Surplus Valuable";

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ValuableObject))]
        [HarmonyPatch(nameof(ValuableObject.DollarValueSetLogic))]
        static void ValuableObjectDollarValueSetLogicPostfix(ValuableObject __instance)
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                if (!MutatorSettings.HandleWithCare.MultiplySurplusValue && __instance.gameObject.name.StartsWith(SurplusValuable))
                {
                    return;
                }
                __instance.dollarValueCurrent /= 2;
                __instance.dollarValueOriginal /= 2;
            }
        }
    }
}
