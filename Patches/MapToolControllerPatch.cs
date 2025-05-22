using HarmonyLib;
using Mutators.Mutators.Behaviours;
using System.Reflection;

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
            if (mutatorDescriptionAnnouncingBehaviour && __instance.Active)
            {
                mutatorDescriptionAnnouncingBehaviour.Show();
            }
        }
    }
}
