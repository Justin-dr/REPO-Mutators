using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Mutators.Enums;
using Mutators.Extensions;
using Mutators.Managers;
using Mutators.Mutators;
using Mutators.Network;
using Mutators.Settings;
using REPOLib.Modules;
using UnityEngine;

namespace Mutators.Patches
{
    [HarmonyPatch(typeof(RunManager))]
    internal class RunManagerPatch
    {
        private static bool _isMoonConfigApplied;
        
        [HarmonyPostfix]
        [HarmonyPatch(nameof(RunManager.Awake))]
        static void RunManagerAwakePostfix()
        {
            if (!_isMoonConfigApplied)
            {
                RepoMutators.Settings.LateBindMoonConfig(RepoMutators.Instance.Config);
                _isMoonConfigApplied = true;
            }
        }

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
                if (!NetworkPrefabs.TryGetNetworkPrefabRef(myPrefabId, out PrefabRef? prefabRef))
                {
                    throw new Exception("Unable to establish Mutators NetworkManager: Could not find PrefabRef with id: " + myPrefabId);
                }

                GameObject? gameObject = NetworkPrefabs.SpawnNetworkPrefab(prefabRef, Vector3.zero, Quaternion.identity);
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
            if (IsInShop())
            {
                mutatorManager.GameState = MutatorsGameState.Shop;
                if (SemiFunc.IsMasterClientOrSingleplayer())
                {
                    MutatorsNetworkManager.Instance.ClearBufferedRPCs();

                    GetAndSendMutator();
                }
            }
            else if (SemiFunc.RunIsLevel())
            {
                RepoMutators.Logger.LogDebug($"Applying patch now for mutator: {mutatorManager.CurrentMutator.Name}");
                mutatorManager.CurrentMutator.Patch();
            }
            else
            {
                mutatorManager.GameState = MutatorsGameState.None;
                if (SemiFunc.RunIsArena())
                {
                    mutatorManager.SetActiveMutator(MutatorSettings.NopMutator.NamespacedName);
                }
            }
        }

        private static void GetAndSendMutator()
        {
            IMutator mutator = MutatorManager.Instance.GetWeightedMutator();

            if (mutator is IMultiMutator multiMutator)
            {
                (IList<string> mutators, IDictionary<string, object> meta) = multiMutator.Format();
                
                MutatorsNetworkManager.Instance.SendActiveMutators(multiMutator, meta);
            }
            else
            {
                MutatorsNetworkManager.Instance.SendActiveMutator(mutator.NamespacedName, mutator.Settings.AsMetadata());
            }
            
        }

        private static bool IsInShop()
        {
            RunManager runManager = RunManager.instance;
            return runManager.levelShop.Any(level => level.name == runManager.levelCurrent.name);
        }
    }
}
