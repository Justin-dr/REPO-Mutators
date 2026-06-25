using HarmonyLib;
using Mutators.Mutators.Behaviours.UI;

namespace Mutators.Patches
{
    [HarmonyPatch(typeof(MapToolController))]
    internal class MapToolControllerPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(MapToolController.Update))]
        static void Postfix(MapToolController __instance)
        {
            MutatorDescriptionAnnouncingBehaviour mutatorDescriptionAnnouncingBehaviour = MutatorDescriptionAnnouncingBehaviour.Instance;
            if (mutatorDescriptionAnnouncingBehaviour && __instance.Active && RepoMutators.Settings.MutatorDescriptionInMapTool && __instance.PlayerAvatar == PlayerAvatar.instance)
            {
                mutatorDescriptionAnnouncingBehaviour.Show();
            }

            if (RepoMutators.Settings.MutatorDisplayToggleType == Settings.ModSettings.MutatorNameToggleType.WithDescription)
            {
                MutatorAnnouncingBehaviour mutatorAnnouncingBehaviour = MutatorAnnouncingBehaviour.instance;
                if (mutatorAnnouncingBehaviour && __instance.Active && RepoMutators.Settings.MutatorDescriptionInMapTool && __instance.PlayerAvatar == PlayerAvatar.instance)
                {
                    mutatorAnnouncingBehaviour.Show();
                }
            }
        }
    }
}
