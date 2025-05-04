using HarmonyLib;
using UnityEngine;

namespace Mutators.Mutators.Patches
{
    internal class ApolloElevenPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(LevelGenerator))]
        [HarmonyPatch(nameof(LevelGenerator.GenerateDone))]
        static void PlayerControllerAwakePostfix()
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

        private static void MakeAllPhysGrabObjectsZeroGravity()
        {
            Collider[] array = Physics.OverlapSphere(new Vector3(0, 10, 0), 200f, LayerMask.GetMask("PhysGrabObject"));
            for (int i = 0; i < array.Length; i++)
            {
                PhysGrabObject physGrabObject = array[i].GetComponentInParent<PhysGrabObject>();

                if (physGrabObject == null) continue;

                physGrabObject.OverrideDrag(0.5f, float.MaxValue);
                physGrabObject.OverrideAngularDrag(0.5f, float.MaxValue);
                physGrabObject.OverrideZeroGravity(float.MaxValue);
            }
        }
    }
}
