using HarmonyLib;
using Mutators.Managers;
using Mutators.Mutators;
using Mutators.Network;
using Photon.Pun;
using UnityEngine;

namespace Mutators.Patches
{
    [HarmonyPatch(typeof(NetworkConnect))]
    internal class NetworkConnectPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnCreatedRoom")]
        static void OnCreatedRoomPostfix()
        {
            var myPrefabId = $"{MyPluginInfo.PLUGIN_GUID}/{RepoMutators.NETWORKMANAGER_NAME}";
            var instance = PhotonNetwork.InstantiateRoomObject(myPrefabId, Vector3.zero, Quaternion.identity);
            instance.SetActive(true);
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnDestroy")]
        static void OnDestroy()
        {
            if (!SemiFunc.IsMultiplayer()) return;

            if (SemiFunc.IsMasterClient())
            {
                MutatorManager mutatorManager = MutatorManager.Instance;
                IMutator mutator = mutatorManager.GetWeightedMutator();
                RepoMutators.Logger.LogDebug($"Picked weighted mutator: {mutator.Name}");

                mutatorManager.CurrentMutator = mutator;
                MutatorsNetworkManager.Instance.SendActiveMutator(mutator.Name);

                RepoMutators.Logger.LogDebug($"Mutator set: {mutator.Name}");
            }
        }
    }
}
