﻿using HarmonyLib;
using Mutators.Mutators.Behaviours.UI;

namespace Mutators.Patches
{
    [HarmonyPatch(typeof(SpectateCamera))]
    internal class SpectateCameraPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(SpectateCamera.LateUpdate))]
        static void SpectateCameraLateUpdatePostfix()
        {
            SpecialActionAnnouncingBehaviour specialActionAnnouncingBehaviour = SpecialActionAnnouncingBehaviour.instance;

            if (specialActionAnnouncingBehaviour)
            {
                specialActionAnnouncingBehaviour.Hide();
            }
        }
    }
}
