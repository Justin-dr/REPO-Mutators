using HarmonyLib;
using Mutators.Managers;
using Mutators.Network;
using Mutators.Settings;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mutators.Mutators.Patches
{
    internal class HandleWithCarePatch
    {
        private const string SurplusValuable = "Surplus Valuable";
        private static readonly IList<ValuableObject> valuableObjects = new List<ValuableObject>();
        private static bool SetupDone = false;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ValuableObject))]
        [HarmonyPatch(nameof(ValuableObject.Awake))]
        static void ValuableObjectAwake(ValuableObject __instance)
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                if (!MutatorSettings.HandleWithCare.MultiplySurplusValue && __instance.gameObject.name.StartsWith(SurplusValuable)) 
                {
                    return;
                }
                RepoMutators.Logger.LogInfo($"{__instance.name}: {__instance.photonView}");
                valuableObjects.Add(__instance);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ValuableObject))]
        [HarmonyPatch(nameof(ValuableObject.DollarValueSetLogic))]
        static void ValuableObjectDollarValueSetLogicPostfix(ValuableObject __instance)
        {
            if (SemiFunc.IsMasterClientOrSingleplayer() && SetupDone)
            {
                if (!MutatorSettings.HandleWithCare.MultiplySurplusValue && __instance.gameObject.name.StartsWith(SurplusValuable))
                {
                    return;
                }
                __instance.dollarValueCurrent *= MutatorSettings.HandleWithCare.ValueMultiplier;
                __instance.dollarValueOriginal *= MutatorSettings.HandleWithCare.ValueMultiplier;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(LevelGenerator))]
        [HarmonyPatch(nameof(LevelGenerator.GenerateDone))]
        static void LevelGeneratorGenerateDonePostfix()
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;
            SetupDone = true;

            foreach (ValuableObject valuableObject in valuableObjects)
            {
                valuableObject.dollarValueCurrent *= MutatorSettings.HandleWithCare.ValueMultiplier;
                valuableObject.dollarValueOriginal *= MutatorSettings.HandleWithCare.ValueMultiplier;

                valuableObject.photonView.RPC("DollarValueSetRPC", RpcTarget.Others, valuableObject.dollarValueCurrent);
            }
            valuableObjects.Clear();
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

        private static void BeforeUnpatchAll()
        {
            SetupDone = false;
        }
    }
}
