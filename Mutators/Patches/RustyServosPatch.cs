using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mutators.Mutators.Patches
{
    internal class RustyServosPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerController))]
        [HarmonyPatch(nameof(PlayerController.Start))]
        static void PlayerControllerStart(PlayerController __instance)
        {
            __instance.JumpCooldown = float.MaxValue;
        }
    }
}
