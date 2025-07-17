using HarmonyLib;
using Mutators.Managers;

namespace Mutators.Patches
{
    [HarmonyPatch(typeof(SemiFunc))]
    internal class SemiFuncPatch
    {
        [HarmonyPostfix]
        
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(nameof(SemiFunc.OnSceneSwitch))]
        static void SemiFuncOnSceneSwitchPostfix(bool _gameOver, bool _leaveGame)
        {
            if (_leaveGame)
            {
                MutatorManager mutatorManager = MutatorManager.Instance;
                mutatorManager.GameState = Enums.MutatorsGameState.None;
                mutatorManager.SetActiveMutator(Mutators.Mutators.NopMutatorName);
            }
        }
    }
}
