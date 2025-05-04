using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Mutators.Managers;
using Mutators.Mutators;
using Mutators.Mutators.Patches;
using Mutators.Network;
using Mutators.Patches;
using Photon.Pun;
using REPOLib;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Mutators;

[BepInDependency(REPOLib.MyPluginInfo.PLUGIN_GUID, REPOLib.MyPluginInfo.PLUGIN_VERSION)]
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.NAME, MyPluginInfo.PLUGIN_VERSION)]
public class RepoMutators : BaseUnityPlugin
{
    internal const string NETWORKMANAGER_NAME = "MutatorsNetworkManager";
    internal static RepoMutators Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger => Instance._logger;
    private ManualLogSource _logger => base.Logger;
    internal Harmony? Harmony { get; set; }

    private void Awake()
    {
        Instance = this;
        
        // Prevent the plugin from being deleted
        this.gameObject.transform.parent = null;
        this.gameObject.hideFlags = HideFlags.HideAndDontSave;

        string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "mutators");

        BundleLoader.LoadBundle(path, assetbundle =>
        {
            AssetStore.Preset = assetbundle.LoadAsset<ExplosionPreset>("explosion default");
            _logger.LogInfo($"Loaded {MyPluginInfo.NAME} asset bundle");
        });

        GameObject myPrefab = new GameObject("RepoMutatorsPrefab")
        {
            hideFlags = HideFlags.HideAndDontSave,
        };
        myPrefab.SetActive(false);
        myPrefab.AddComponent<PhotonView>();
        myPrefab.AddComponent<MutatorsNetworkManager>();

        string myPrefabId = $"{MyPluginInfo.PLUGIN_GUID}/{NETWORKMANAGER_NAME}";
        REPOLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(myPrefabId, myPrefab);

        Patch();

        Logger.LogInfo($"{Info.Metadata.GUID} v{Info.Metadata.Version} has loaded!");

        BundleLoader.OnAllBundlesLoaded += BundleLoader_OnAllBundlesLoaded;
    }

    private void BundleLoader_OnAllBundlesLoaded()
    {
        _logger.LogDebug("Initializing default mutators.");
        MutatorManager.Instance.InitializeDefaultMutators();
    }

    internal void Patch()
    {
        Harmony ??= new Harmony(Info.Metadata.GUID);
        Harmony.PatchAll(typeof(NetworkConnectPatch));
        Harmony.PatchAll(typeof(RunManagerPatch));
        // Harmony.PatchAll(typeof(ApolloElevenPatch));
    }

    internal void Unpatch()
    {
        Harmony?.UnpatchSelf();
        foreach (Mutator mutator in MutatorManager.Instance.RegisteredMutators.Values)
        {
            mutator.Unpatch();
        }
    }
}