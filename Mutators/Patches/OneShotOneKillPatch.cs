using HarmonyLib;
using Mutators.Extensions;
using Mutators.Settings;

namespace Mutators.Mutators.Patches
{

    internal class OneShotOneKillPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerHealthGrab))]
        [HarmonyPatch(nameof(PlayerHealthGrab.Start))]
        static void PlayerHealthGrabStart(PlayerHealthGrab __instance)
        {
            __instance.enabled = false;
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
        [HarmonyPriority(Priority.Normal + 50)]
        [HarmonyPatch(typeof(PlayerDeathHead))]
        [HarmonyPatch(nameof(PlayerDeathHead.Update))]
        static void PlayerDeathHeadUpdatePostfix(PlayerDeathHead __instance)
        {
            if (!__instance.triggered || !MutatorSettings.OneShotOneKill.InstaReviveInTruckOrExtraction || SemiFunc.IsNotMasterClient()) return;

            if (__instance.roomVolumeCheck.inTruck)
            {
                DoRevive(__instance, true);
            }
            else if (__instance.roomVolumeCheck.inExtractionPoint)
            {
                DoRevive(__instance, false);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerHealth))]
        [HarmonyPatch(nameof(PlayerHealth.Hurt))]
        static void PlayerHealthHurtPrefix(PlayerHealth __instance, ref int damage, ref bool savingGrace)
        {
            if (__instance.godMode || damage < 1) return;
            damage = __instance.maxHealth;
            savingGrace = false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerHealth))]
        [HarmonyPatch(nameof(PlayerHealth.HurtOther))]
        static void PlayerHealthHurtOtherPrefix(PlayerHealth __instance, ref int damage, ref bool savingGrace)
        {
            if (__instance.godMode || damage < 1) return;
            damage = __instance.maxHealth;
            savingGrace = false;
        }

        private static void DoRevive(PlayerDeathHead playerDeathHead, bool inTruck)
        {
            int health = MutatorSettings.OneShotOneKill.InstaReviveHealth == 0
                ? playerDeathHead.playerAvatar.playerHealth.maxHealth - 1
                : MutatorSettings.OneShotOneKill.InstaReviveHealth - 1;

            playerDeathHead.playerAvatar.Revive(inTruck);

            if (health > 0)
            {
                playerDeathHead.playerAvatar.playerHealth.HealOther(health, true);
            }
        }
    }
}
