using HarmonyLib;

namespace Mutators.Mutators.Patches
{
    internal class DuckThisPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(EnemyDuck))]
        [HarmonyPatch(nameof(EnemyDuck.UpdateState))]
        static void EnemyDuckUpdateStatePrefix(ref EnemyDuck.State __state)
        {
            //RepoMutators.Logger.LogInfo($"Duck This - State: {__state}");
            if (__state == EnemyDuck.State.GoToPlayer || __state == EnemyDuck.State.GoToPlayerOver || __state == EnemyDuck.State.GoToPlayerUnder)
            {
                RepoMutators.Logger.LogInfo($"Duck This - Setting state to transform");
                //__state = EnemyDuck.State.Transform;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EnemyDuck))]
        [HarmonyPatch(nameof(EnemyDuck.Update))]
        static void EnemyDuckUpdateStateTransformPrefix(EnemyDuck __instance)
        {
            RepoMutators.Logger.LogInfo($"Duck This - State: {__instance.currentState} - Impulse: {__instance.stateImpulse}");
            if (__instance.currentState == EnemyDuck.State.GoToPlayer || __instance.currentState == EnemyDuck.State.GoToPlayerOver || __instance.currentState == EnemyDuck.State.GoToPlayerUnder)
            {
                __instance.UpdateState(EnemyDuck.State.AttackStart);
            }
            if (__instance.currentState == EnemyDuck.State.DeTransform)
            {
                __instance.UpdateState(EnemyDuck.State.Leave);
            }
        }
    }
}
