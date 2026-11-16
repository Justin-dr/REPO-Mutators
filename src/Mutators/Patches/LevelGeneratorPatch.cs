using HarmonyLib;
using Mutators.Enums;
using Mutators.Managers;

namespace Mutators.Patches
{
    [HarmonyPatch(typeof(LevelGenerator))]
    internal class LevelGeneratorPatch
    {
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(nameof(LevelGenerator.GenerateDoneApply))]
        static void LevelGeneratorGenerateDoneApplyPrefix()
        {
            if (SemiFunc.RunIsLevel())
            {
                MutatorManager.Instance.GameState = MutatorsGameState.LevelReady;
            }
        }
        
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        [HarmonyPatch(nameof(LevelGenerator.GenerateDoneApply))]
        static void LevelGeneratorGenerateDoneApplyPostfix()
        {
            if (SemiFunc.RunIsLevel())
            {
                MutatorManager.Instance.GameState = MutatorsGameState.LevelGenerated;
            }
        }
    }
}
