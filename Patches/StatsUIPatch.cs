using HarmonyLib;
using Mutators.Mutators.Behaviours;

namespace Mutators.Patches
{
    [HarmonyPatch(typeof(StatsUI))]
    internal class StatsUIPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(StatsUI.Show))]
        static void Postfix()
        {
            MutatorDescriptionAnnouncingBehaviour mutatorDescriptionAnnouncingBehaviour = MutatorDescriptionAnnouncingBehaviour.instance;
            if (mutatorDescriptionAnnouncingBehaviour)
            {
                mutatorDescriptionAnnouncingBehaviour.Show();
            }
        }
    }
}
