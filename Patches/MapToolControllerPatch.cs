using HarmonyLib;
using Mutators.Mutators.Behaviours;

namespace Mutators.Patches
{
    [HarmonyPatch(typeof(MapToolController))]
    internal class MapToolControllerPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(MapToolController.Update))]
        static void Postfix(MapToolController __instance)
        {
            MutatorDescriptionAnnouncingBehaviour mutatorDescriptionAnnouncingBehaviour = MutatorDescriptionAnnouncingBehaviour.instance;
            if (mutatorDescriptionAnnouncingBehaviour && __instance.Active && RepoMutators.Settings.MutatorDescriptionInMapTool && __instance.PlayerAvatar == PlayerAvatar.instance)
            {
                
                mutatorDescriptionAnnouncingBehaviour.Show();
            }
        }
    }
}
