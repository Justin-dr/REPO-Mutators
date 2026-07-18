using HarmonyLib;
using Mutators.Managers;

namespace Mutators.Patches
{
    [HarmonyPatch(typeof(MenuManager))]
    internal class MenuManagerPatch
    {
        [HarmonyPostfix]
        [HarmonyPriority(Priority.High)]
        [HarmonyPatch(nameof(MenuManager.PageOpen))]
        private static void PageOpenPostfix(MenuPageIndex menuPageIndex)
        {
            if (menuPageIndex == MenuPageIndex.Main)
            {
                LevelManager.Instance.RestoreLevels();
            }
        }
    }
}