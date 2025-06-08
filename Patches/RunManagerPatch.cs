using HarmonyLib;
using Mutators.Managers;
using Mutators.Mutators;
using Mutators.Network;
using Mutators.Settings.Specific;
using System.Collections.Generic;
using UnityEngine;

namespace Mutators.Patches
{
    [HarmonyPatch(typeof(RunManager))]
    internal class RunManagerPatch
    {
        private static readonly ISet<string> vanillaLevelNames = new HashSet<string>()
        {
            { "Level - Artic" },
            { "Level - Manor" },
            { "Level - Wizard" },
            { "Level - Museum" }
        };

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

        [HarmonyPrefix]
        [HarmonyPatch(nameof(RunManager.SetRunLevel))]
        static void RunManagerChangeLevelPrefix(RunManager __instance, ref Level ___previousRunLevel)
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;
            if (MutatorManager.Instance.CurrentMutator.Settings is ILevelRemovingMutatorSettings settings && !settings.AllowCustomLevels)
            {
                RepoMutators.Logger.LogInfo("Removing custom levels from selection");
                __instance.levels.RemoveAll(l => !vanillaLevelNames.Contains(l.name));

                if (__instance.levels.Count == 1)
                {
                    ___previousRunLevel = null!;
                }
                else if (__instance.levels.Count == 0)
                {
                    ___previousRunLevel = null!;
                    RepoMutators.Logger.LogError("Attempted to start a run with 0 available levels, please revisit your mod settings!");
                    RepoMutators.Logger.LogError("There must be at least one level available to choose from!");
                }
            }
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
