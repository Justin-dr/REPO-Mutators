using System.Collections.Generic;
using HarmonyLib;
using Mutators.Extensions;
using Mutators.Network;
using Mutators.Settings;

namespace Mutators.Mutators.Patches
{
    internal class UltraViolencePatch
    {
        private static bool _keepLightsOn = MutatorSettings.UltraViolence.KeepOnLight;
        internal const string KeepLightsOn = "keep-lights-on";

        private static void OnMetadataChanged(IDictionary<string, object> metadata)
        {
            _keepLightsOn = metadata.Get<bool>(KeepLightsOn);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(LevelGenerator))]
        [HarmonyPatch(nameof(LevelGenerator.GenerateDone))]
        static void LevelGeneratorGenerateDonePostfix()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                MutatorsNetworkManager.Instance.SendMetadata(
                    MutatorSettings.UltraViolence.NamespacedName,
                    new Dictionary<string, object> { { KeepLightsOn, MutatorSettings.UltraViolence.KeepOnLight } }
                );
            }
        }

        [HarmonyPostfix]
        [HarmonyPriority(Priority.LowerThanNormal)]
        [HarmonyPatch(typeof(EnemyDirector))]
        [HarmonyPatch(nameof(EnemyDirector.Start))]
        static void EnemyDirectorAmountSetupPostfix(EnemyDirector __instance)
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

            __instance.DisableEnemies(MutatorSettings.UltraViolence);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ExtractionPoint))]
        [HarmonyPatch(nameof(ExtractionPoint.ActivateTheFirstExtractionPointAutomaticallyWhenAPlayerLeaveTruck))]
        static void ExtractionPointActivateTheFirstExtractionPointAutomaticallyWhenAPlayerLeaveTruckPostfix()
        {
            RoundDirector roundDirector = RoundDirector.instance;

            int extractionPoints = roundDirector.extractionPoints;
            roundDirector.extractionPointsCompleted = extractionPoints;

            roundDirector.ExtractionCompletedAllCheck();

            roundDirector.extractionPointsCompleted = 0;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(LightManager))]
        [HarmonyPatch(nameof(LightManager.Update))]
        static void LightManagerUpdatePrefix(out bool __state)
        {
            __state = ShouldHideFakeFinalStateFromLightManager();
            if (__state)
            {
                RoundDirector.instance.allExtractionPointsCompleted = false;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(LightManager))]
        [HarmonyPatch(nameof(LightManager.Update))]
        static void LightManagerUpdatePostfix(bool __state)
        {
            if (__state)
            {
                RoundDirector.instance.allExtractionPointsCompleted = true;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerAvatar))]
        [HarmonyPatch(nameof(PlayerAvatar.FinalHeal))]
        static bool PlayerAvatarFinalHealPrefix() // Disabled truck heal until all extraction points have been completed
        {
            RoundDirector roundDirector = RoundDirector.instance;
            return roundDirector.extractionPointsCompleted >= roundDirector.extractionPoints;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TruckHealer))]
        [HarmonyPatch(nameof(TruckHealer.StateUpdate))]
        static bool TruckHealerStateUpdatePrefixPrefix() // Disable truck healing light
        {
            RoundDirector roundDirector = RoundDirector.instance;
            return roundDirector.extractionPointsCompleted >= roundDirector.extractionPoints;
        }

        private static bool ShouldHideFakeFinalStateFromLightManager()
        {
            return _keepLightsOn && ShouldHideFakeFinalState();
        }

        internal static bool ShouldHideFakeFinalState()
        {
            RoundDirector roundDirector = RoundDirector.instance;
            if (!roundDirector || !roundDirector.allExtractionPointsCompleted)
            {
                return false;
            }

            int extractionPoints = roundDirector.extractionPoints;
            int extractionPointsCompleted = roundDirector.extractionPointsCompleted;

            if (!roundDirector.extractionPointCurrent)
            {
                return extractionPointsCompleted < extractionPoints;
            }

            bool finalExtractionStarted = extractionPointsCompleted >= extractionPoints - 1;
            ExtractionPoint.State currentState = roundDirector.extractionPointCurrent.currentState;

            return !finalExtractionStarted ||
                   (currentState != ExtractionPoint.State.Extracting && currentState != ExtractionPoint.State.Complete);
        }

        private static void BeforeUnpatchAll()
        {
            _keepLightsOn = MutatorSettings.UltraViolence.KeepOnLight;
        }
    }
}
