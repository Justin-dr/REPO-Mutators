using HarmonyLib;
using Mutators.Extensions;
using Mutators.Managers;
using Mutators.Mutators;
using Mutators.Network;
using Mutators.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
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
                if (!REPOLib.Modules.NetworkPrefabs.TryGetNetworkPrefabRef(myPrefabId, out PrefabRef? prefabRef))
                {
                    throw new System.Exception("Unable to establish Mutators NetworkManager: Could not find PrefabRef with id: " + myPrefabId);
                }

                GameObject? gameObject = REPOLib.Modules.NetworkPrefabs.SpawnNetworkPrefab(prefabRef, Vector3.zero, Quaternion.identity);
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
            if (MutatorManager.Instance.CurrentMutator.Settings is ILevelRemovingMutatorSettings settings)
            {

                if (!settings.AllowCustomLevels)
                {
                    __instance.levels.RemoveAll(l => !vanillaLevelNames.Contains(l.name));
                }

                if (settings.ExcludedLevels.Count > 0)
                {
                    ISet<string> excludedSet = new HashSet<string>(
                        settings.ExcludedLevels.Select(level => level.StartsWith("level - ", StringComparison.OrdinalIgnoreCase) ? level.ToLowerInvariant(): ("level - " + level).ToLowerInvariant())
                    );

                    __instance.levels.RemoveAll(level => excludedSet.Contains(level.name.ToLowerInvariant()));
                }


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
            if (IsInShop())
            {
                mutatorManager.GameState = Enums.MutatorsGameState.Shop;
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
                mutatorManager.GameState = Enums.MutatorsGameState.None;
                if (SemiFunc.RunIsArena())
                {
                    mutatorManager.SetActiveMutator(Mutators.Mutators.NopMutatorName);
                }
            }
        }

        private static void GetAndSendMutator()
        {
            IMutator mutator = MutatorManager.Instance.GetWeightedMutator();

            RepoMutators.Logger.LogInfo($"{string.Join(", ", MutatorManager.Instance.RegisteredMutators.Select(x => x.Key))}");

            if (mutator is IMultiMutator multiMutator)
            {
                var fromattedMutator = multiMutator.Format();

                MutatorsNetworkManager.Instance.SendActiveMutators(
                    fromattedMutator.mutators,
                    fromattedMutator.meta
                );
            }
            else
            {
                MutatorsNetworkManager.Instance.SendActiveMutator(mutator.Name, mutator.Settings.AsMetadata());
            }
            
        }

        private static bool IsInShop()
        {
            RunManager runManager = RunManager.instance;
            return runManager.levelCurrent.name == runManager.levelShop.name;
        }
    }
}
