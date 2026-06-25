using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using Mutators.Assets.Loaders;
using Mutators.Extensions;
using Mutators.Logging.Loggers;
using Mutators.Managers;
using Mutators.Mutators;
using Mutators.Network;
using Mutators.Patches;
using Mutators.Providers.Random;
using Mutators.Providers.Semiwork;
using Mutators.Rules;
using Mutators.Rules.Loaders;
using Mutators.Rules.Loaders.Json;
using Mutators.Rules.Loaders.Strategies;
using Mutators.Rules.Registries;
using Mutators.Services.Selection;
using Mutators.Services.Selection.Strategies;
using Mutators.Settings;
using Photon.Pun;
using REPOLib.Modules;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Mutators;

[BepInDependency("Vippy.ScalerCore", "1.0.2")]
[BepInDependency(REPOLib.MyPluginInfo.PLUGIN_GUID, REPOLib.MyPluginInfo.PLUGIN_VERSION)]
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class RepoMutators : BaseUnityPlugin
{
    /// <summary>
    /// The path to Mutator's config file.
    /// </summary>
    public static string ConfigPath => Path.Combine(Paths.ConfigPath, MyPluginInfo.PLUGIN_NAME);

    // Assets
    internal const string MainScenePath = "Assets/Scenes/Main/Main.unity";
    internal const string NETWORKMANAGER_NAME = "MutatorsNetworkManager";

    // Meta
    internal const string MUTATOR_OVERRIDES = "mutatorOverrides";

    internal static RepoMutators Instance { get; private set; } = null!;
    internal static ModSettings Settings { get; private set; } = null!;
    internal new static IMutatorsLogger Logger { get; private set; } = new NullMutatorsLogger();

    internal Harmony? Harmony { get; set; }
    
    /// <summary>
    /// Event that is fired when multi-mutator rules are loaded.
    /// </summary>
    public static event Action<IRuleLoader<Func<IReadOnlyCollection<string>, string, bool>>>? OnLoadMultiMutatorRules;
    
    /// <summary>
    /// Event that is fired when single-mutator rules are loaded.
    /// </summary>
    public static event Action<IRuleLoader<Predicate<string>>>? OnLoadSingleMutatorRules;
    
    private RepoMutators()
    {
        
    }
    
    private void Awake()
    {
        Instance = this;
        Logger = new MutatorsLogger(base.Logger);
        // Prevent the plugin from being deleted
        this.gameObject.transform.parent = null;
        this.gameObject.hideFlags = HideFlags.HideAndDontSave;

        Settings = new ModSettings(Config);
        MutatorSettings.Initialize(Config);
        
        InitializeDependencyContainer(new NopMutator(MutatorSettings.NopMutator));

        GameObject myPrefab = new GameObject("RepoMutatorsPrefab")
        {
            hideFlags = HideFlags.HideAndDontSave,
        };
        myPrefab.SetActive(false);
        myPrefab.AddComponent<PhotonView>();
        myPrefab.AddComponent<MutatorsNetworkManager>();

        string myPrefabId = $"{MyPluginInfo.PLUGIN_GUID}/{NETWORKMANAGER_NAME}";
        NetworkPrefabs.RegisterNetworkPrefab(myPrefabId, myPrefab);

        ScheduleAssetLoad();

        SceneManager.sceneLoaded += (scene, _) => {
            if (scene.path != MainScenePath) return;
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;
            if (RunManager.instance.levelCurrent == RunManager.instance.levelMainMenu)
            {
                if (MutatorsNetworkManager.Instance != null && MutatorsNetworkManager.Instance.gameObject != null)
                {
                    Destroy(MutatorsNetworkManager.Instance.gameObject);
                }
                return;
            }
            if (MutatorsNetworkManager.Instance != null) return;
            if (!SemiFunc.RunIsLobbyMenu()) return;

            Logger.LogDebug("Reviving network manager");
            if (!NetworkPrefabs.TryGetNetworkPrefabRef(myPrefabId, out PrefabRef? prefabRef))
            {
                throw new Exception("Unable to establish Mutators NetworkManager: Could not find PrefabRef with id: " + myPrefabId);
            } 
            NetworkPrefabs.SpawnNetworkPrefab(prefabRef, Vector3.zero, Quaternion.identity);

            MutatorManager mutatorManager = MutatorManager.Instance;
            Logger.LogInfo($"{string.Join(", ", mutatorManager.RegisteredMutators.Select(x => $"{x.Key}: {x.Value.Settings.Weight}"))}");
            IMutator mutator = mutatorManager.GetWeightedMutator();
            Logger.LogDebug($"Picked weighted mutator: {mutator.Name}");

            mutatorManager.CurrentMutator = mutator;

            if (mutator is IMultiMutator multiMutator)
            {
                var formattedMutator = multiMutator.Format();
                MutatorsNetworkManager.Instance!.SendActiveMutators(multiMutator, formattedMutator.meta);
            }
            else
            {
                MutatorsNetworkManager.Instance!.SendActiveMutator(mutator.NamespacedName, mutator.Settings.AsMetadata());
            }

            Logger.LogDebug($"Mutator set: {mutator.Name}");
        };

        MutatorManager.Instance.GameStateChanged += (gameState) => Logger.LogDebug($"Changed Mutators gamestate to {gameState}");

        Patch();

        Logger.LogInfo($"{Info.Metadata.GUID} v{Info.Metadata.Version} has loaded!");

        Logger.LogDebug("Initializing default mutators.");
        MutatorManager.Instance.InitializeDefaultMutators();

        OnLoadSingleMutatorRules += ruleLoader =>
        {
            ruleLoader.AddRuleStrategy(new ExclusionRuleLoadingStrategy());
            
            ruleLoader.AddDefaultRule(new JsonMutatorRule("exclude-null-signal", SingleMutatorRuleType.Exclusion, [MutatorSettings.NullSignal.NamespacedName]));
        };

        OnLoadMultiMutatorRules += ruleLoader =>
        {
            ruleLoader.AddRuleStrategy(new MultiExclusionRuleLoadingStrategy());
            ruleLoader.AddRuleStrategy(new MutualExclusionRuleLoadingStrategy());
            ruleLoader.AddRuleStrategy(new RequiresAmountOfOtherMutatorsRuleLoadingStrategy());
            
            IDictionary<string, object> arguments = new Dictionary<string, object> { { "other-mutators-required-amount", 2 } };
            
            ruleLoader.AddDefaultRules(
                new JsonMutatorRule("handle-with-care-less-is-more", MultiMutatorRuleType.MutualExclusion, [MutatorSettings.HandleWithCare.NamespacedName, MutatorSettings.LessIsMore.NamespacedName]),
                new JsonMutatorRule("protect-the-weak-protect-the-president", MultiMutatorRuleType.MutualExclusion, [MutatorSettings.ProtectTheWeak.NamespacedName, MutatorSettings.ProtectThePresident.NamespacedName]),
                new JsonMutatorRule("the-floor-is-lava-one-shot-one-kill", MultiMutatorRuleType.MutualExclusion, [MutatorSettings.TheFloorIsLava.NamespacedName, MutatorSettings.OneShotOneKill.NamespacedName]),
                new JsonMutatorRule("the-floor-is-lava-rusty-servos", MultiMutatorRuleType.MutualExclusion, [MutatorSettings.TheFloorIsLava.NamespacedName, MutatorSettings.RustyServos.NamespacedName]),
                new JsonMutatorRule("size-matters-rusty-servos", MultiMutatorRuleType.MutualExclusion, [MutatorSettings.SizeMatters.NamespacedName, MutatorSettings.RustyServos.NamespacedName]),
                new JsonMutatorRule("null-signal-required-amount-of-other-mutators", MultiMutatorRuleType.RequiresAmountOfOtherMutators, [MutatorSettings.NullSignal.NamespacedName], arguments: arguments)
            );
        };
    }

    private void InitializeDependencyContainer(NopMutator nopMutator)
    {
        DI.Container.AddSingleton<IMutatorsLogger>(Logger)
            .AddSingleton<IRandomProvider, RandomProvider>()
            .AddSingleton<ISemiFuncProvider, SemiFuncProvider>()
            .AddSingleton(
                new GeneratedMultiMutatorSelectionRulesRegistry()
             )
            .AddSingleton<SingleMutatorSelectionRulesRegistry>()
            .AddSingleton<IRepeatSelectionTracker, RepeatSelectionTracker>()
            .AddSingleton(Settings.MoonMutatorSettings)
            .AddSingleton(Settings.RandomMutatorSettings)
            .AddSingleton(nopMutator)
            .AddSingleton<NoneScalingSelectionStrategy>()
            .AddSingleton<MoonScalingSelectionStrategy>()
            .AddSingleton<RandomScalingSelectionStrategy>()
            .AddSingleton<IMutatorSelectionService, MutatorSelectionService>()
            .AddSingleton<MutatorManager>();
    }

    private static void ScheduleAssetLoad()
    {
        UnityAction<Scene, LoadSceneMode> sceneLoaded = null!;
        sceneLoaded = (scene, _) =>
        {
            if (scene.path != MainScenePath) return;
            List<BaseGameAssetLoader> loaders =
            [
                new LaserAssetLoader(),
                new ExplosionAssetLoader()
            ];
            
            loaders.ForEach(loader => loader.Load());
            SceneManager.sceneLoaded -= sceneLoaded;
        };
        
        SceneManager.sceneLoaded += sceneLoaded;
    }

    private void LoadRules()
    {
        SingleMutatorRuleLoader singleRulesLoader = new SingleMutatorRuleLoader();
        MultiMutatorRuleLoader multiRulesLoader = new MultiMutatorRuleLoader();
        
        OnLoadSingleMutatorRules?.Invoke(singleRulesLoader);
        OnLoadMultiMutatorRules?.Invoke(multiRulesLoader);
        
        IDictionary<string, Predicate<string>> singleRules = singleRulesLoader.Load();
        IDictionary<string, Func<IReadOnlyCollection<string>, string, bool>> multiRules = multiRulesLoader.Load();

        SingleMutatorSelectionRulesRegistry singleRegistry = DI.Container.Resolve<SingleMutatorSelectionRulesRegistry>();
        GeneratedMultiMutatorSelectionRulesRegistry multiRegistry = DI.Container.Resolve<GeneratedMultiMutatorSelectionRulesRegistry>();
        
        singleRules.ForEach(rule => singleRegistry.Register(rule.Key, rule.Value));
        multiRules.ForEach(rule => multiRegistry.Register(rule.Key, rule.Value));
        
        OnLoadSingleMutatorRules = null;
        OnLoadMultiMutatorRules = null;
    }

    private void Start()
    {
        MutatorManager.Instance.InitializeMultiMutators();
        LoadRules();
    }

    private void Patch()
    {
        bool hasSpawnManager = Chainloader.PluginInfos.Values.Any(x => x.Metadata.GUID == "soundedsquash.spawnmanager");

        Harmony ??= new Harmony(Info.Metadata.GUID);
        Harmony.PatchAll(typeof(RunManagerPatch));
        Harmony.PatchAll(typeof(LoadingUIPatch));
        Harmony.PatchAll(typeof(MapToolControllerPatch));
        Harmony.PatchAll(typeof(SemiFuncPatch));
        Harmony.PatchAll(typeof(MenuPagePatch));
        Harmony.PatchAll(typeof(SpectateCameraPatch));
        Harmony.PatchAll(typeof(LevelGeneratorPatch));
        Harmony.PatchAll(typeof(MenuManagerPatch));
        Harmony.PatchAll(typeof(HealthUIPatch));
        Harmony.PatchAll(typeof(MapBacktrackPatch));

        if (!hasSpawnManager)
        {
            Harmony.PatchAll(typeof(EnemyDirectorPatch));
        }
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
