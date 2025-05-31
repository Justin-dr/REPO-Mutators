using HarmonyLib;
using Mutators.Mutators.Behaviours.UI;

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
            MutatorDescriptionAnnouncingBehaviour mutatorDescriptionAnnouncingBehaviour = MutatorDescriptionAnnouncingBehaviour.Instance;
            TargetPlayerAnnouncingBehaviour targetPlayerAnnouncingBehaviour = TargetPlayerAnnouncingBehaviour.instance;
            SpecialActionAnnouncingBehaviour specialActionAnnouncingBehaviour = SpecialActionAnnouncingBehaviour.instance;

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

            if (specialActionAnnouncingBehaviour)
            {
                specialActionAnnouncingBehaviour.Hide();
            }
        }
    }
}
