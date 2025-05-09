using HarmonyLib;

namespace Mutators.Mutators.Patches
{

    internal class OneShotOneKillPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerHealth))]
        [HarmonyPatch(nameof(PlayerHealth.Hurt))]
        static void PlayerHealthHurtPrefix(PlayerHealth __instance, ref int damage, ref bool savingGrace)
        {
            damage = __instance.maxHealth;
            savingGrace = false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerHealth))]
        [HarmonyPatch(nameof(PlayerHealth.HurtOther))]
        static void PlayerHealthHurtOtherPrefix(PlayerHealth __instance, ref int damage, ref bool savingGrace)
        {
            damage = __instance.maxHealth;
            savingGrace = false;
        }
    }
}
