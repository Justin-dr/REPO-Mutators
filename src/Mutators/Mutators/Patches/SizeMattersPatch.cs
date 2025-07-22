using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Mutators.Announcements;
using Mutators.Extensions;
using Mutators.Managers;
using Mutators.Settings;
using Mutators.Settings.Specific;
using ScalerCore;

namespace Mutators.Mutators.Patches
{
    internal class SizeMattersPatch
    {
        private static ScaleOptions scaleOptions = ScaleOptions.Default;
        private static bool _late;

        static void OnMetadataChanged(IDictionary<string, object> metadata)
        {
            IList<string> targets = new List<string> { "Players" };

            if (metadata.Get<bool>(SizeMattersMutatorSettings.ScaleValuablesKey))
            {
                targets.Add("valuables");
            }
            
            if (metadata.Get<bool>(SizeMattersMutatorSettings.ScaleEnemiesKey))
            {
                targets.Add("enemies");
            }
            
            if (metadata.Get<bool>(SizeMattersMutatorSettings.ScaleCartsKey))
            {
                targets.Add("carts");
            }

            if (MutatorAnnouncingBag.Instance.TryGetAnnouncement(MutatorSettings.SizeMatters.NamespacedName, out MutatorAnnouncement? announcement))
            {
                announcement.UpdateBaseDescription($"{FormatScaleTargets(targets)} are shrunk");
            }
            else
            {
                RepoMutators.Logger.LogWarning($"[{MutatorSettings.SizeMatters.MutatorName}] Unable to find announcement, could not update description.");
            }

        }

        private static string FormatScaleTargets(IList<string> targets) => targets.Count switch
        {
            1 => targets[0],
            2 => string.Join(" and ", targets),
            _ => string.Join(", ", targets.Take(targets.Count - 1)) + ", and " + targets.Last()
        };
        

        [HarmonyPostfix]
        [HarmonyPatch(typeof(LevelGenerator))]
        [HarmonyPatch(nameof(LevelGenerator.GenerateDone))]
        static void LevelGeneratorGenerateDonePostfix()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                SetupScaleOptions();
                
                if (!MutatorManager.Instance.HasCurrentMutator(MutatorSettings.NullSignal.NamespacedName))
                {
                    SemiFunc.PlayerGetAll().ForEach(ScalePlayer);
                    _late = true;
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerAvatar))]
        [HarmonyPatch(nameof(PlayerAvatar.Start))]
        static void PlayerAvatarStartPrefix(PlayerAvatar __instance)
        {
            if (_late && SemiFunc.IsMasterClientOrSingleplayer())
            {
                ScalePlayer(__instance);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ExtractionPoint))]
        [HarmonyPatch(nameof(ExtractionPoint.ActivateTheFirstExtractionPointAutomaticallyWhenAPlayerLeaveTruck))]
        static void ExtractionPointActivateTheFirstExtractionPointAutomaticallyWhenAPlayerLeaveTruckPostfix()
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

            if (MutatorManager.Instance.HasCurrentMutator(MutatorSettings.NullSignal.NamespacedName))
            {
                SemiFunc.PlayerGetAll().ForEach(ScalePlayer);
                _late = true;
            }
            
            foreach (PhysGrabObject physGrabObject in RoundDirector.instance.physGrabObjects.ToList())
            {
                if (!physGrabObject || !ShouldScale(physGrabObject)) continue;

                ScaleManager.Apply(physGrabObject.gameObject, scaleOptions);
            }
        }

        [HarmonyPostfix]
        [HarmonyPriority(Priority.LowerThanNormal)]
        [HarmonyPatch(typeof(PhysGrabObject))]
        [HarmonyPatch(nameof(PhysGrabObject.Start))]
        static void PhysGrabObjectStartPostfix(PhysGrabObject __instance)
        {
            if (!_late || !SemiFunc.IsMasterClientOrSingleplayer() || !ShouldScale(__instance)) return;
            
            ScaleManager.Apply(__instance.gameObject, scaleOptions);
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(EnemyParent))]
        [HarmonyPatch(nameof(EnemyParent.SpawnRPC))]
        static void EnemyParentSpawnRPCPostfix(EnemyParent __instance)
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer() || !MutatorSettings.SizeMatters.ScaleEnemies) return;
            RepoMutators.Logger.LogInfo($"Applying scale to {__instance.enemyName}");
            ScaleManager.Apply(__instance.Enemy.Rigidbody.gameObject, scaleOptions);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerAvatar))]
        [HarmonyPatch(nameof(PlayerAvatar.ReviveRPC))]
        static void PlayerAvatarReviveRPCPostfix(PlayerAvatar __instance)
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

            ScalePlayer(__instance);
        }


        private static void ScalePlayer(PlayerAvatar playerAvatar)
        {
            ScaleManager.Apply(playerAvatar.gameObject, scaleOptions);
        }

        private static void SetupScaleOptions()
        {
            scaleOptions.IgnoreBonkExpand = true;
            scaleOptions.RejectExternalApply = true;
            scaleOptions.Duration = 0;
            scaleOptions.AllowedTargets = ScaleTargets.All;
        }

        private static bool ShouldScale(PhysGrabObject physGrabObject)
        {
            return IsScalableEnemy(physGrabObject) || IsScalableCart(physGrabObject) || IsScalableItem(physGrabObject);
        }

        private static bool IsScalableEnemy(PhysGrabObject physGrabObject)
        {
            return MutatorSettings.SizeMatters.ScaleEnemies && physGrabObject.isEnemy;
        }

        private static bool IsScalableCart(PhysGrabObject physGrabObject)
        {
            return MutatorSettings.SizeMatters.ScaleCarts && IsCart(physGrabObject);
        }

        private static bool IsScalableItem(PhysGrabObject physGrabObject)
        {
            return MutatorSettings.SizeMatters.ScaleValuables
                   && !IsCart(physGrabObject)
                   && (physGrabObject.isValuable
                       || physGrabObject.GetComponent<CosmeticWorldObject>()
                       || physGrabObject.GetComponent<ItemAttributes>());
        }

        private static bool IsCart(PhysGrabObject physGrabObject)
        {
            return physGrabObject.isCart || physGrabObject.GetComponent<PhysGrabCart>();
        }

        static void AfterUnpatchAll()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                scaleOptions = ScaleOptions.Default;
            }
            _late = false;
        }

    }
}
