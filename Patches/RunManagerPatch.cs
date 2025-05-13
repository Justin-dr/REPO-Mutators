using HarmonyLib;
using Mutators.Managers;
using Mutators.Mutators;
using Mutators.Network;
using Photon.Pun;
using UnityEngine;

namespace Mutators.Patches
{
    [HarmonyPatch(typeof(RunManager))]
    internal class RunManagerPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(RunManager.ChangeLevel))]
        static void RunManagerChangeLevelPostfix()
        {
            if (SemiFunc.IsMultiplayer() && SemiFunc.IsNotMasterClient()) return;

            RepoMutators.Logger.LogDebug("RunManagerPatch Host only");

            ApplyPatch();
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(RunManager.UpdateLevel))]
        static void RunManagerUpdateLevelPostfix()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer()) return;

            RepoMutators.Logger.LogDebug("RunManagerPatch Client only");

            ApplyPatch();
        }

        private static void ApplyPatch()
        {
            MutatorManager mutatorManager = MutatorManager.Instance;
            if (SemiFunc.RunIsShop() && SemiFunc.IsMasterClientOrSingleplayer())
            {
                MutatorsNetworkManager mutatorsNetworkManager = MutatorsNetworkManager.Instance;
                mutatorsNetworkManager.ClearBufferedRPCs();

                IMutator mutator = mutatorManager.GetWeightedMutator();
                mutatorsNetworkManager.SendActiveMutator(mutator.Name);
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
