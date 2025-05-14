using HarmonyLib;
using Mutators.Settings;
using UnityEngine;

namespace Mutators.Mutators.Patches
{
    internal class ApolloElevenPatch
    {

        [HarmonyPostfix]
        [HarmonyPatch(typeof(LevelGenerator))]
        [HarmonyPatch(nameof(LevelGenerator.GenerateDone))]
        static void LevelGeneratorGenerateDonePostfix()
        {
            if (SemiFunc.RunIsLevel())
            {
                PlayerController.instance.AntiGravity(float.MaxValue);

                if (SemiFunc.IsMasterClientOrSingleplayer())
                {
                    MakeAllPhysGrabObjectsZeroGravity();
                }
            }
            else
            {
                PlayerController.instance.AntiGravity(0);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PhysGrabObjectImpactDetector))]
        [HarmonyPatch(nameof(PhysGrabObjectImpactDetector.Update))]
        static void LevelGeneratorGenerateDonePostfix(PhysGrabObjectImpactDetector __instance)
        {
            if (__instance.isNotValuable) return;

            if (__instance.inCart && !__instance.inCartPrevious)
            {
                __instance.physGrabObject.OverrideDrag(0.5f, 0.1f);
                __instance.physGrabObject.OverrideAngularDrag(0.5f, 0.1f);
                __instance.physGrabObject.OverrideZeroGravity(0.1f);
            }
            else if (!__instance.inCart && __instance.inCartPrevious)
            {
                MakeObjectZeroGravity(__instance.physGrabObject);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerController))]
        [HarmonyPatch(nameof(PlayerController.AntiGravity))]
        static bool PlayControllerAntiGravityPrefix(float _timer)
        {
            return _timer == float.MaxValue;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PhysGrabObject))]
        [HarmonyPatch(nameof(PhysGrabObject.OverrideDrag))]
        static bool PhysGrabObjectOverrideDragPrefix(PhysGrabObject __instance, float value, float time)
        {
            if (__instance.impactDetector.inCart)
            {
                return true;
            }

            return time == float.MaxValue;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PhysGrabObject))]
        [HarmonyPatch(nameof(PhysGrabObject.OverrideAngularDrag))]
        static bool PhysGrabObjectOverrideAngularDragPrefix(PhysGrabObject __instance, float value, float time)
        {
            if (__instance.impactDetector.inCart)
            {
                return true;
            }

            return time == float.MaxValue;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PhysGrabObject))]
        [HarmonyPatch(nameof(PhysGrabObject.OverrideZeroGravity))]
        static bool PhysGrabObjectOverrideZeroGravityPrefix(PhysGrabObject __instance, float time)
        {
            if (__instance.impactDetector.inCart)
            {
                return true;
            }

            return time == float.MaxValue;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerController))]
        [HarmonyPatch(nameof(PlayerController.FixedUpdate))]
        static void PlayerControllerFixedUpdatePrefix(PlayerController __instance)
        {
            if (Input.GetKey(MutatorSettings.ApolloEleven.DownwardsKey))
            {
                __instance.rb.AddForce(Vector3.down * 50f, ForceMode.Force);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(RoundDirector))]
        [HarmonyPatch(nameof(RoundDirector.PhysGrabObjectAdd))]
        static void RoundDirectorPhysGrabObjectAddPostfix(PhysGrabObject _physGrabObject)
        {
            MakeObjectZeroGravity(_physGrabObject);
        }

        private static void MakeAllPhysGrabObjectsZeroGravity()
        {
            foreach (PhysGrabObject physGrabObject in RoundDirector.instance.physGrabObjects)
            {
                MakeObjectZeroGravity(physGrabObject);
            }
        }

        private static void MakeObjectZeroGravity(PhysGrabObject physGrabObject)
        {
            if (!MutatorSettings.ApolloEleven.ApplyToEnemies && physGrabObject.GetComponent<EnemyRigidbody>()) return;

            physGrabObject.OverrideDrag(0.5f, float.MaxValue);
            physGrabObject.OverrideAngularDrag(0.5f, float.MaxValue);
            physGrabObject.OverrideZeroGravity(float.MaxValue);
        }
    }
}
