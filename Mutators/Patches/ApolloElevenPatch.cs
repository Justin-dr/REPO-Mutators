using HarmonyLib;
using Mutators.Settings;
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
