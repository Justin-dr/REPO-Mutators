using HarmonyLib;
using Mutators.Utility;

namespace Mutators.Patches
{
    // This class purely exists to ensure MapVote compatibility
    [HarmonyPatch(typeof(HealthUI))]
    internal class HealthUIPatch
    {
        [HarmonyPostfix]
        [HarmonyPriority(Priority.High)]
        [HarmonyPatch(nameof(HealthUI.Start))]
        static void HealthUIStartPostfix()
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

            LevelRemovalUtils.RemoveLevels();
        }
    }
}