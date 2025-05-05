using HarmonyLib;
using Mutators.Mutators.Behaviours;
using Unity.VisualScripting;

namespace Mutators.Mutators.Patches
{
    internal class DuckThisPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(EnemyDuck))]
        [HarmonyPatch(nameof(EnemyDuck.Update))]
        static void EnemyDuckUpdatePrefix(EnemyDuck __instance)
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                DuckThisBehaviour noticeBehaviour = __instance.GetOrAddComponent<DuckThisBehaviour>();

                if (__instance.currentState == EnemyDuck.State.GoToPlayer || __instance.currentState == EnemyDuck.State.GoToPlayerOver || __instance.currentState == EnemyDuck.State.GoToPlayerUnder)
                {
                    if (noticeBehaviour.CanNotice())
                    {
                        __instance.UpdateState(EnemyDuck.State.AttackStart);
                    }
                }
                if (__instance.currentState == EnemyDuck.State.DeTransform)
                {
                    __instance.playerTarget = null;
                    __instance.UpdateState(EnemyDuck.State.Idle);
                    noticeBehaviour._noticeCooldown = 120f;
                }
            }
        }
    }
}
