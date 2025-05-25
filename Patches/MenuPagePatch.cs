using HarmonyLib;
using Mutators.Mutators.Behaviours;

namespace Mutators.Patches
{
    [HarmonyPatch(typeof(MenuPage))]
    internal class MenuPagePatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(MenuPage.LockAndHide))]
        static void MenuPageLockAndHidePostfix()
        {
            MutatorAnnouncingBehaviour mutatorAnnouncingBehaviour = MutatorAnnouncingBehaviour.instance;
            MutatorDescriptionAnnouncingBehaviour mutatorDescriptionAnnouncingBehaviour = MutatorDescriptionAnnouncingBehaviour.instance;
            TargetPlayerAnnouncingBehaviour targetPlayerAnnouncingBehaviour = TargetPlayerAnnouncingBehaviour.instance;

            if (mutatorAnnouncingBehaviour)
            {
                mutatorAnnouncingBehaviour.Hide();
            }

            if (mutatorDescriptionAnnouncingBehaviour)
            {
                mutatorDescriptionAnnouncingBehaviour.Hide();
            }

            if (targetPlayerAnnouncingBehaviour)
            {
                targetPlayerAnnouncingBehaviour.Hide();
            }
        }
    }
}
