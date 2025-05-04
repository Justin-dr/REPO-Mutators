using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Mutators.Mutators.Patches
{
    internal class UltraViolencePatch
    {
        private static readonly FieldInfo _extractionPointsField = AccessTools.Field(typeof(RoundDirector), "extractionPoints");
        private static readonly FieldInfo _extractionPointsCompletedField = AccessTools.Field(typeof(RoundDirector), "extractionPointsCompleted");

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ExtractionPoint))]
        [HarmonyPatch(nameof(ExtractionPoint.ActivateTheFirstExtractionPointAutomaticallyWhenAPlayerLeaveTruck))]
        static void LevelGeneratorGenerateDonePostfix()
        {
            RepoMutators.Logger.LogInfo("Trigger UltraViolence");

            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                int extractionPoints = (int) _extractionPointsField.GetValue(RoundDirector.instance);
                _extractionPointsCompletedField.SetValue(RoundDirector.instance, extractionPoints);

                RoundDirector.instance.ExtractionCompletedAllCheck();

                _extractionPointsCompletedField.SetValue(RoundDirector.instance, 0);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerAvatar))]
        [HarmonyPatch(nameof(PlayerAvatar.FinalHeal))]
        static bool PlayerAvatarFinalHealPrefix() // Disabled truck heal until all extraction points have been completed
        {
            //TODO: Maybe find a way to disable the healing light too, but can't be too bothered right now
            int extractionPoints = (int)_extractionPointsField.GetValue(RoundDirector.instance);
            int extractionPointsCompleted = (int) _extractionPointsCompletedField.GetValue(RoundDirector.instance);
            return extractionPointsCompleted >= extractionPoints;
        }
    }
}
