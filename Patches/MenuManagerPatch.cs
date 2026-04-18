using HarmonyLib;
using Mutators.Extensions;
using Mutators.Managers;
using Mutators.Settings;

namespace Mutators.Patches;

[HarmonyPatch(typeof(MenuManager))]
internal class MenuManagerPatch
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(nameof(MenuManager.PageOpen))]
    private static void PageOpenPostfix(MenuPageIndex menuPageIndex)
    {
        if (menuPageIndex == MenuPageIndex.Lobby && SemiFunc.IsMasterClient())
        {
            if (MutatorManager.Instance.CurrentMutator.Settings is ILevelRemovingMutatorSettings settings)
            {
                settings.RemoveLevels(true);
            }
        }

        if (menuPageIndex == MenuPageIndex.Main)
        {
            LevelManager.Instance.RestoreLevels();
        }
    }
}