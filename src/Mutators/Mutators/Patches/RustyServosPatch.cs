using HarmonyLib;

namespace Mutators.Mutators.Patches
{
    internal class RustyServosPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PhysGrabber))]
        [HarmonyPatch(nameof(PhysGrabber.Start))]
        static void PhysGrabberStartPostfix(PhysGrabber __instance)
        {
            if (__instance != PhysGrabber.instance) return;
            __instance.grabRange += 3f;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerController))]
        [HarmonyPatch(nameof(PlayerController.Start))]
        static void PlayerControllerStartPostfix(PlayerController __instance)
        {
            __instance.JumpCooldown = float.MaxValue;
        }
    }
}
