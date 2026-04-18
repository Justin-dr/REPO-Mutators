using HarmonyLib;
using Mutators.Extensions;
using Mutators.Managers;
using Mutators.Settings;

namespace Mutators.Patches;

[HarmonyPatch(typeof(HealthUI))]
public class HealthUIPatch
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(nameof(HealthUI.Start))]
    static void HealthUIStartPostfix()
    {
        if (!SemiFunc.IsMasterClientOrSingleplayer()) return;
        if (MutatorManager.Instance.CurrentMutator.Settings is ILevelRemovingMutatorSettings settings)
        {
            settings.RemoveLevels();
        }
    }
}