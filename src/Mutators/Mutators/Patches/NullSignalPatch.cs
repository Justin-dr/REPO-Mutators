using HarmonyLib;
using Mutators.Announcements;
using Mutators.Mutators.Behaviours.UI;

namespace Mutators.Mutators.Patches
{
    internal class NullSignalPatch
    {
        private const int Priority = HarmonyLib.Priority.Last + 5;
        
        [HarmonyPostfix]
        [HarmonyPriority(Priority)]
        [HarmonyPatch(typeof(MutatorAnnouncement))]
        [HarmonyPatch(nameof(MutatorAnnouncement.GetName))]
        static void MutatorAnnouncementNamePostfix(ref string __result)
        {
            __result = Mutators.NullSignalName;
        }
        
        [HarmonyPostfix]
        [HarmonyPriority(Priority)]
        [HarmonyPatch(typeof(MutatorAnnouncement))]
        [HarmonyPatch(nameof(MutatorAnnouncement.GetDescription))]
        static void MutatorAnnouncementDescriptionPostfix(ref string __result)
        {
            __result = Mutators.NullSignalDescription;
        }

        [HarmonyPostfix]
        [HarmonyPriority(HarmonyLib.Priority.VeryLow - 50)]
        [HarmonyPatch(typeof(LoadingUI))]
        [HarmonyPatch(nameof(LoadingUI.StopLoading))]
        static void LoadingUIStopLoadingPostfix()
        {
            TargetPlayerAnnouncingBehaviour targetPlayerAnnouncingBehaviour = TargetPlayerAnnouncingBehaviour.Instance;
            SpecialActionAnnouncingBehaviour specialActionAnnouncingBehaviour = SpecialActionAnnouncingBehaviour.Instance;
            
            if (targetPlayerAnnouncingBehaviour)
            {
                targetPlayerAnnouncingBehaviour.gameObject.SetActive(false);
            }

            if (specialActionAnnouncingBehaviour)
            {
                specialActionAnnouncingBehaviour.gameObject.SetActive(false);
            }
        }
    }
}