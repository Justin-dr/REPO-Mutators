using HarmonyLib;
using Mutators.Extensions;
using Mutators.Managers;
using Mutators.Network;
using Mutators.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Mutators.Mutators.Patches
{
    internal class UltraViolencePatch
    {
        private static readonly Func<RoundDirector, int> _getExtractionPoints = CreateFieldGetter<RoundDirector, int>("extractionPoints");
        private static readonly Func<RoundDirector, int> _getExtractionPointsCompleted = CreateFieldGetter<RoundDirector, int>("extractionPointsCompleted");
        private static readonly Action<RoundDirector, int> _setExtractionPointsCompleted = CreateFieldSetter<RoundDirector, int>("extractionPointsCompleted");

        private static bool _keepLightsOn = MutatorSettings.UltraViolence.KeepOnLight;

        private static void OnMetadataChanged(IDictionary<string, object> metadata)
        {
            _keepLightsOn = metadata.Get<bool>("keepLightsOn");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(LevelGenerator))]
        [HarmonyPatch(nameof(LevelGenerator.GenerateDone))]
        static void LevelGeneratorGenerateDonePostfix()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                MutatorsNetworkManager.Instance.SendMetadata(new Dictionary<string, object>() { { "keepLightsOn", MutatorSettings.UltraViolence.KeepOnLight } });
            }
        }

        [HarmonyPostfix]
        [HarmonyPriority(Priority.LowerThanNormal)]
        [HarmonyPatch(typeof(EnemyDirector))]
        [HarmonyPatch(nameof(EnemyDirector.Start))]
        static void EnemyDirectorAmountSetupPostfix(EnemyDirector __instance)
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

            __instance.DisableEnemies();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ExtractionPoint))]
        [HarmonyPatch(nameof(ExtractionPoint.ActivateTheFirstExtractionPointAutomaticallyWhenAPlayerLeaveTruck))]
        static void ExtractionPointActivateTheFirstExtractionPointAutomaticallyWhenAPlayerLeaveTruckPostfix()
        {
            int extractionPoints = _getExtractionPoints(RoundDirector.instance);
            _setExtractionPointsCompleted(RoundDirector.instance, extractionPoints);

            RoundDirector.instance.ExtractionCompletedAllCheck();

            _setExtractionPointsCompleted(RoundDirector.instance, 0);

            if (_keepLightsOn)
            {
                RoundDirector.instance.allExtractionPointsCompleted = false;
            }
            else
            {
                MutatorsNetworkManager.Instance.Run(TurnOffLightsLate());
            }

            
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(LightManager))]
        [HarmonyPatch(nameof(LightManager.TurnOffLights))]
        static bool LightManagerTurnOffLightsPrefix()
        {
            if (_keepLightsOn)
            {
                return RoundDirector.instance.allExtractionPointsCompleted;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerAvatar))]
        [HarmonyPatch(nameof(PlayerAvatar.FinalHeal))]
        static bool PlayerAvatarFinalHealPrefix() // Disabled truck heal until all extraction points have been completed
        {
            int extractionPoints = _getExtractionPoints(RoundDirector.instance);
            int extractionPointsCompleted = _getExtractionPointsCompleted(RoundDirector.instance);
            return extractionPointsCompleted >= extractionPoints;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TruckHealer))]
        [HarmonyPatch(nameof(TruckHealer.StateUpdate))]
        static bool TruckHealerStateUpdatePrefixPrefix() // Disable truck healing light
        {
            int extractionPoints = _getExtractionPoints(RoundDirector.instance);
            int extractionPointsCompleted = _getExtractionPointsCompleted(RoundDirector.instance);
            return extractionPointsCompleted >= extractionPoints;
        }

        private static Func<T, R> CreateFieldGetter<T, R>(string fieldName)
        {
            var field = AccessTools.Field(typeof(T), fieldName) ?? throw new Exception($"Field {fieldName} not found on {typeof(T)}");

            var param = Expression.Parameter(typeof(T), "instance");
            var fieldAccess = Expression.Field(param, field);
            var lambda = Expression.Lambda<Func<T, R>>(fieldAccess, param);
            return lambda.Compile();
        }

        private static Action<T, V> CreateFieldSetter<T, V>(string fieldName)
        {
            var field = AccessTools.Field(typeof(T), fieldName) ?? throw new Exception($"Field {fieldName} not found on {typeof(T)}");

            var targetExp = Expression.Parameter(typeof(T), "instance");
            var valueExp = Expression.Parameter(typeof(V), "value");

            var fieldExp = Expression.Field(targetExp, field);
            var assignExp = Expression.Assign(fieldExp, valueExp);

            var lambda = Expression.Lambda<Action<T, V>>(assignExp, targetExp, valueExp);
            return lambda.Compile();
        }

        private static IEnumerator TurnOffLightsLate()
        {
            yield return null;
            RoundDirector.instance.allExtractionPointsCompleted = false;
        }

        private static void BeforeUnpatchAll()
        {
            _keepLightsOn = MutatorSettings.UltraViolence.KeepOnLight;
        }
    }
}
