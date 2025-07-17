using HarmonyLib;
using Mutators.Managers;

namespace Mutators.Patches
{
    [HarmonyPatch(typeof(LevelGenerator))]
    internal class LevelGeneratorPatch
    {
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(nameof(LevelGenerator.GenerateDone))]
        static void LevelGeneratorGenerateDonePrefix()
        {
            if (SemiFunc.RunIsLevel())
            {
                MutatorManager.Instance.GameState = Enums.MutatorsGameState.LevelGenerated;
            }
        }
    }
}
