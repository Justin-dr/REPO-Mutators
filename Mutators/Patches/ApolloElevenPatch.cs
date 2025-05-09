using HarmonyLib;
using System.Threading;
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
        static bool PhysGrabObjectOverrideDragPrefix(float value, float time)
        {
            return time == float.MaxValue;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PhysGrabObject))]
        [HarmonyPatch(nameof(PhysGrabObject.OverrideAngularDrag))]
        static bool PhysGrabObjectOverrideAngularDragPrefix(float value, float time)
        {
            return time == float.MaxValue;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PhysGrabObject))]
        [HarmonyPatch(nameof(PhysGrabObject.OverrideZeroGravity))]
        static bool PhysGrabObjectOverrideZeroGravityPrefix(float time)
        {
            return time == float.MaxValue;
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
            Collider[] array = Physics.OverlapSphere(new Vector3(0, 10, 0), 300f, LayerMask.GetMask("PhysGrabObject"));
            for (int i = 0; i < array.Length; i++)
            {
                PhysGrabObject physGrabObject = array[i].GetComponentInParent<PhysGrabObject>();

                if (physGrabObject == null) continue;

                MakeObjectZeroGravity(physGrabObject);
            }
        }

        private static void MakeObjectZeroGravity(PhysGrabObject physGrabObject)
        {
            physGrabObject.OverrideDrag(0.5f, float.MaxValue);
            physGrabObject.OverrideAngularDrag(0.5f, float.MaxValue);
            physGrabObject.OverrideZeroGravity(float.MaxValue);
        }
    }
}
