using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Mutators.Managers;
using Mutators.Mutators;
using Mutators.Mutators.Behaviours;
using Mutators.Network;
using Mutators.Patches;
using Mutators.Settings;
using Photon.Pun;
using REPOLib;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mutators;

[BepInDependency(REPOLib.MyPluginInfo.PLUGIN_GUID, REPOLib.MyPluginInfo.PLUGIN_VERSION)]
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.NAME, MyPluginInfo.PLUGIN_VERSION)]
public class RepoMutators : BaseUnityPlugin
{
    internal const string MainScenePath = "Assets/Scenes/Main/Main.unity";
    internal const string NETWORKMANAGER_NAME = "MutatorsNetworkManager";
    internal static RepoMutators Instance { get; private set; } = null!;
    internal static ModSettings Settings { get; private set; } = null!;
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

            GameObject firingMyLaser = assetbundle.LoadAsset<GameObject>("FiringMyLaser");
            firingMyLaser.SetActive(false);
            firingMyLaser.AddComponent<PhotonView>();
            firingMyLaser.AddComponent<LaserFiringBehaviour>();

            REPOLib.Modules.NetworkPrefabs.RegisterNetworkPrefab($"{MyPluginInfo.PLUGIN_GUID}/FiringMyLaser", firingMyLaser);

            _logger.LogInfo($"Loaded {MyPluginInfo.NAME} asset bundle");
        });

        Settings = new ModSettings(Config);
        MutatorSettings.Initialize(Config);

        GameObject myPrefab = new GameObject("RepoMutatorsPrefab")
        {
            hideFlags = HideFlags.HideAndDontSave,
        };
        myPrefab.SetActive(false);
        myPrefab.AddComponent<PhotonView>();
        myPrefab.AddComponent<MutatorsNetworkManager>();

        string myPrefabId = $"{MyPluginInfo.PLUGIN_GUID}/{NETWORKMANAGER_NAME}";
        REPOLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(myPrefabId, myPrefab);

        SceneManager.sceneLoaded += (scene, loadSceneMode) => {
            if (scene.path != MainScenePath) return;
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;
            if (RunManager.instance.levelCurrent == RunManager.instance.levelMainMenu)
            {
                if (MutatorsNetworkManager.Instance != null && MutatorsNetworkManager.Instance.gameObject != null)
                {
                    Object.Destroy(MutatorsNetworkManager.Instance.gameObject);
                }
                return;
            }
            if (MutatorsNetworkManager.Instance != null) return;
            if (!SemiFunc.RunIsLobbyMenu()) return;

            Logger.LogDebug("Reviving network manager");
            REPOLib.Modules.NetworkPrefabs.SpawnNetworkPrefab(myPrefabId, Vector3.zero, Quaternion.identity);

            MutatorManager mutatorManager = MutatorManager.Instance;
            IMutator mutator = mutatorManager.GetWeightedMutator();
            Logger.LogDebug($"Picked weighted mutator: {mutator.Name}");

            mutatorManager.CurrentMutator = mutator;
            MutatorsNetworkManager.Instance!.SendActiveMutator(mutator.Name);

            Logger.LogDebug($"Mutator set: {mutator.Name}");
        };

        Patch();

        Logger.LogInfo($"{Info.Metadata.GUID} v{Info.Metadata.Version} has loaded!");

        Logger.LogDebug("Initializing default mutators.");
        MutatorManager.Instance.InitializeDefaultMutators();
    }

    internal void Patch()
    {
        Harmony ??= new Harmony(Info.Metadata.GUID);
        Harmony.PatchAll(typeof(RunManagerPatch));
        Harmony.PatchAll(typeof(LoadingUIPatch));
        Harmony.PatchAll(typeof(MapToolControllerPatch));
        Harmony.PatchAll(typeof(SemiFuncPatch));
        Harmony.PatchAll(typeof(MenuPagePatch));
        Harmony.PatchAll(typeof(SpectateCameraPatch));
    }

    internal void Unpatch()
    {
        Harmony?.UnpatchSelf();
        foreach (IMutator mutator in MutatorManager.Instance.RegisteredMutators.Values)
        {
            mutator.Unpatch();
        }
    }
}