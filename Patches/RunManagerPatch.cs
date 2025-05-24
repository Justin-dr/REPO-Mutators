using HarmonyLib;
using Mutators.Managers;
using Mutators.Mutators;
using Mutators.Network;
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


            if (!SemiFunc.IsMultiplayer() && !SemiFunc.RunIsLobbyMenu() && MutatorsNetworkManager.Instance == null)
            {
                RepoMutators.Logger.LogDebug($"Spawning singleplayer NetworkManager");
                string myPrefabId = $"{MyPluginInfo.PLUGIN_GUID}/{RepoMutators.NETWORKMANAGER_NAME}";
                GameObject? gameObject = REPOLib.Modules.NetworkPrefabs.SpawnNetworkPrefab(myPrefabId, Vector3.zero, Quaternion.identity);
                gameObject?.SetActive(true);

                GetAndSendMutator();
            }

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
                MutatorsNetworkManager.Instance.ClearBufferedRPCs();

                GetAndSendMutator();
            }
            else if (SemiFunc.RunIsLevel())
            {
                RepoMutators.Logger.LogDebug($"Applying patch now for mutator: {mutatorManager.CurrentMutator.Name}");
                mutatorManager.CurrentMutator.Patch();
            }
            else if (SemiFunc.RunIsArena())
            {
                mutatorManager.SetActiveMutator(Mutators.Mutators.NopMutatorName);
            }
        }

        private static void GetAndSendMutator()
        {
            IMutator mutator = MutatorManager.Instance.GetWeightedMutator();
            MutatorsNetworkManager.Instance.SendActiveMutator(mutator.Name);
        }
    }
}
