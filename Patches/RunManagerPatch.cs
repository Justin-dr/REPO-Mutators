using HarmonyLib;
using Mutators.Managers;
using Mutators.Mutators;
using Mutators.Network;
using static RunManager;

namespace Mutators.Patches
{
    [HarmonyPatch(typeof(RunManager))]
    internal class RunManagerPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(RunManager.ChangeLevel))]
        static void RunManagerChangeLevelPostfix()
        {
            MutatorManager mutatorManager = MutatorManager.Instance;
            if (SemiFunc.RunIsShop())
            {
                IMutator mutator = mutatorManager.GetWeightedMutator();
                RepoMutators.Logger.LogDebug($"Set mutator to {mutator.Name}, applying patch later");
                mutatorManager.SetActiveMutator(mutator.Name, false);
                MutatorsNetworkManager.Instance.SendActiveMutator(mutator.Name);
            }
            else if (SemiFunc.RunIsLevel())
            {
                RepoMutators.Logger.LogDebug($"Applying patch now for mutator: {mutatorManager.CurrentMutator.Name}");
                mutatorManager.CurrentMutator.Patch();
            }
            else if (SemiFunc.RunIsArena())
            {
                mutatorManager.SetActiveMutator(Mutators.Mutators.NopMutator);
            }
        }
    }
}
