using System.Collections;
using System.Reflection;
using BepInEx.Configuration;
using Mutators.Managers;
using Mutators.Mutators;
using Mutators.Providers.Random;
using Mutators.Providers.Semiwork;
using Mutators.Rules.Registries;
using Mutators.Services.Selection;
using Mutators.Services.Selection.Strategies;
using Mutators.Settings;
using Mutators.Tests.Logging.Loggers;
using Mutators.Tests.Providers.Random;
using Mutators.Tests.Providers.Semiwork;
using Mutators.Utility.Config;

namespace Mutators.Tests.Managers
{
    internal abstract class MutatorManagerTestBase(ModSettings.MultiMutatorScalingType scalingType)
    {
        private const string RandomSection = "Multi-Mutators - Random";

        private static readonly Dictionary<byte, MoonConfigEntries> MoonConfigEntriesByLevel = new();
        private static readonly Dictionary<byte, RandomAmountConfigEntries> RandomAmountConfigEntriesByAmount = new();
        private static bool _settingsInitialized;
        private static ConfigFile _config = null!;
        private static ConfigEntry<int> _nopChance = null!;
        private static ConfigEntry<string> _scalingMode = null!;
        private static ConfigEntry<int> _randomMinimumAmount = null!;
        private static ConfigEntry<int> _randomMaximumAmount = null!;

        protected MutatorManager mutatorManager = null!;
        protected TestRandomProvider randomProvider = null!;
        protected TestLogger logger = null!;
        private TestSemiFuncProvider semiFuncProvider = null!;


        [SetUp]
        public void Setup()
        {
            SetScalingType(scalingType);
            SetConfiguredNopChance(0);
            SetRandomScalingDefaults();

            randomProvider = new TestRandomProvider();
            semiFuncProvider = new TestSemiFuncProvider();
            logger = new TestLogger();

            SetLogger(logger);
            EnsureTinyDiInitialized();
            mutatorManager = CreateManager();
        }

        protected void SetRandomFloatValue(float value)
        {
            randomProvider.QueueFloat(value);
        }

        protected MutatorManager CreateManager(params IMutator[] mutators)
        {
            NopMutator nopMutator = new(MutatorSettings.NopMutator);
            IRepeatSelectionTracker repeatSelectionTracker = new RepeatSelectionTracker();
            GeneratedMultiMutatorSelectionRulesRegistry multiRegistry = new();
            SingleMutatorSelectionRulesRegistry singleRegistry = new();

            NoneScalingSelectionStrategy noneStrategy = new(
                multiRegistry,
                singleRegistry,
                repeatSelectionTracker,
                randomProvider,
                nopMutator
            );
            MoonScalingSelectionStrategy moonStrategy = new(
                multiRegistry,
                singleRegistry,
                RepoMutators.Settings.MoonMutatorSettings,
                semiFuncProvider,
                repeatSelectionTracker,
                randomProvider,
                nopMutator
            );
            RandomScalingSelectionStrategy randomStrategy = new(
                multiRegistry,
                singleRegistry,
                RepoMutators.Settings.RandomMutatorSettings,
                repeatSelectionTracker,
                randomProvider,
                nopMutator
            );
            IMutatorSelectionService selectionService = new MutatorSelectionService(
                noneStrategy,
                moonStrategy,
                randomStrategy,
                repeatSelectionTracker,
                randomProvider,
                nopMutator
            );
            MutatorManager manager = new(
                selectionService,
                semiFuncProvider,
                multiRegistry,
                singleRegistry,
                nopMutator
            );

            foreach (IMutator mutator in mutators)
            {
                manager.RegisterMutator(mutator);
            }

            return manager;
        }

        private static void SetLogger(TestLogger testLogger)
        {
            PropertyInfo loggerProperty = typeof(RepoMutators).GetProperty(
                "Logger",
                BindingFlags.Static | BindingFlags.NonPublic
            )!;

            loggerProperty.GetSetMethod(true)!.Invoke(null, [testLogger]);
        }

        private void EnsureTinyDiInitialized()
        {
            if (DI.Container.RegisteredTypes.Contains(typeof(MutatorManager)))
            {
                return;
            }

            NopMutator nopMutator = new(MutatorSettings.NopMutator);

            DI.Container.AddSingleton<IRandomProvider>(randomProvider)
                .AddSingleton<ISemiFuncProvider>(semiFuncProvider)
                .AddSingleton<GeneratedMultiMutatorSelectionRulesRegistry>()
                .AddSingleton<SingleMutatorSelectionRulesRegistry>()
                .AddSingleton<IRepeatSelectionTracker, RepeatSelectionTracker>()
                .AddSingleton(RepoMutators.Settings.MoonMutatorSettings)
                .AddSingleton(RepoMutators.Settings.RandomMutatorSettings)
                .AddSingleton(nopMutator)
                .AddSingleton<NoneScalingSelectionStrategy>()
                .AddSingleton<MoonScalingSelectionStrategy>()
                .AddSingleton<RandomScalingSelectionStrategy>()
                .AddSingleton<IMutatorSelectionService, MutatorSelectionService>()
                .AddSingleton<MutatorManager>();
        }

        protected void SetRandomFloatValues(params float[] values)
        {
            foreach (float value in values)
            {
                randomProvider.QueueFloat(value);
            }
        }

        protected void SetRandomIntValue(int value)
        {
            randomProvider.QueueInt(value);
        }

        protected void SetRandomIntValues(params int[] values)
        {
            foreach (int value in values)
            {
                randomProvider.QueueInt(value);
            }
        }

        protected void SetMoonLevel(int moonLevel)
        {
            semiFuncProvider.QueueMoonLevel(moonLevel);
        }

        protected void SetMoonLevels(params int[] moonLevels)
        {
            foreach (int moonLevel in moonLevels)
            {
                semiFuncProvider.QueueMoonLevel(moonLevel);
            }
        }

        protected static void SetConfiguredNopChance(int chance)
        {
            InitializeSharedSettings();

            _nopChance.Value = chance;
        }
        
        protected static void ConfigureMoonRange(int moonLevel, int minimumMutators, int maximumMutators, int generatedChance)
        {
            InitializeSharedSettings();

            byte moon = (byte)moonLevel;
            MoonConfigEntries entries = GetOrCreateMoonConfigEntries(moon);
            entries.Minimum.Value = minimumMutators;
            entries.Maximum.Value = maximumMutators;
            entries.GeneratedChance.Value = generatedChance;

            ConfigRange<int> range = new(entries.Minimum, entries.Maximum);
            object moonSettings = new ModSettings.MoonSetting(range, entries.GeneratedChance);
            
            ModSettings.MoonSettings settings = RepoMutators.Settings.MoonMutatorSettings;
            FieldInfo moonRangesField = typeof(ModSettings.MoonSettings).GetField(
                "_multiMutatorMoonRanges",
                BindingFlags.Instance | BindingFlags.NonPublic
            )!;

            if (moonRangesField.GetValue(settings) is not IDictionary ranges)
            {
                Type rangesType = typeof(Dictionary<,>).MakeGenericType(
                    typeof(byte),
                    typeof(ModSettings.MoonSetting)
                );
                ranges = (IDictionary)Activator.CreateInstance(rangesType)!;

                moonRangesField.SetValue(settings, ranges);

            }
            
            ranges[moon] = moonSettings;
        }

        protected static void ConfigureRandomAmountRange(int minimumMutators, int maximumMutators)
        {
            InitializeSharedSettings();

            _randomMinimumAmount.Value = minimumMutators;
            _randomMaximumAmount.Value = maximumMutators;
        }

        protected static void SetAllRandomAmountWeights(int weight)
        {
            InitializeSharedSettings();

            for (byte amount = 1; amount <= ModSettings.MaximumGeneratedActiveSubMutators; amount++)
            {
                GetOrCreateRandomAmountConfigEntries(amount).Weight.Value = weight;
            }
        }

        protected static void SetRandomAmountWeight(int amount, int weight)
        {
            InitializeSharedSettings();

            GetOrCreateRandomAmountConfigEntries((byte)amount).Weight.Value = weight;
        }

        protected static void SetRandomGeneratedChance(int amount, int chance)
        {
            InitializeSharedSettings();

            if (amount < 2)
            {
                throw new ArgumentException("Generated chance is only configured for two or more mutators.", nameof(amount));
            }

            GetOrCreateRandomAmountConfigEntries((byte)amount).GeneratedChance!.Value = chance;
        }

        private static void SetRandomScalingDefaults()
        {
            InitializeSharedSettings();

            _randomMinimumAmount.Value = 1;
            _randomMaximumAmount.Value = 5;
            for (byte amount = 1; amount <= ModSettings.MaximumGeneratedActiveSubMutators; amount++)
            {
                RandomAmountConfigEntries entries = GetOrCreateRandomAmountConfigEntries(amount);
                entries.Weight.Value = 50;
                if (entries.GeneratedChance != null)
                {
                    entries.GeneratedChance.Value = 50;
                }
            }
        }

        private static ConfigFile CreateSharedTestConfig()
        {
            ConfigFile config = new(
                Path.Combine(TestContext.CurrentContext.WorkDirectory, $"mutators-test-{Guid.NewGuid():N}.cfg"),
                false
            )
            {
                SaveOnConfigSet = false
            };

            return config;
        }

        protected void Register(params IMutator[] mutators)
        {
            foreach (IMutator mutator in mutators)
            {
                mutatorManager.RegisterMutator(mutator);
            }
        }
        
        private static void SetScalingType(ModSettings.MultiMutatorScalingType scalingType)
        {
            InitializeSharedSettings();

            _scalingMode.Value = scalingType.ToString();
            RepoMutators.Settings.CacheKeys();
        }

        // Everything below this looks a bit shady, so let me explain:
        // Before this, settings were bound to an actual config.
        // However, this caused the test suite's execution time to jump from 88ms to 6s.
        // Additionally, it wrote a lot of files to disk
        // This way we thus speed up our tests substantially
        private static void InitializeSharedSettings()
        {
            if (_settingsInitialized) return;

            _config = CreateSharedTestConfig();
            _nopChance = _config.Bind("No Mutator", "Chance (%)", 0);
            _scalingMode = _config.Bind("Multi-Mutators", "Scaling Mode", "None");

            MutatorSettings.Initialize(_config);
            SetRepoMutatorsSettings(new ModSettings(_config));

            _nopChance.Value = 0;
            _randomMinimumAmount = _config.Bind(RandomSection, "Minimum amount of Mutators", 1);
            _randomMaximumAmount = _config.Bind(RandomSection, "Maximum amount of Mutators", 5);
            _settingsInitialized = true;
        }

        private static void SetRepoMutatorsSettings(ModSettings settings)
        {
            FieldInfo repoSettingsField = typeof(RepoMutators).GetField(
                "<Settings>k__BackingField",
                BindingFlags.Static | BindingFlags.NonPublic
            )!;

            repoSettingsField.SetValue(null, settings);
        }

        private static MoonConfigEntries GetOrCreateMoonConfigEntries(byte moon)
        {
            if (MoonConfigEntriesByLevel.TryGetValue(moon, out MoonConfigEntries? entries))
            {
                return entries;
            }

            string section = "Test Multi-Mutators - " + (moon == 0 ? "No Moon" : $"Moon {moon}");
            entries = new MoonConfigEntries(
                _config.Bind(section, "Minimum Mutators", 0),
                _config.Bind(section, "Maximum Mutators", 0),
                _config.Bind(section, "Generated Multi-Mutator Chance", 0)
            );

            MoonConfigEntriesByLevel[moon] = entries;
            return entries;
        }

        private static RandomAmountConfigEntries GetOrCreateRandomAmountConfigEntries(byte amount)
        {
            if (RandomAmountConfigEntriesByAmount.TryGetValue(amount, out RandomAmountConfigEntries? entries))
            {
                return entries;
            }

            ConfigEntry<int> weight = _config.Bind(RandomSection, $"{amount} Mutators - Weight", 50);
            ConfigEntry<int>? generatedChance = amount <= 1
                ? null
                : _config.Bind(RandomSection, $"{amount} Mutators - Generated Multi-Mutator Chance (%)", 50);

            entries = new RandomAmountConfigEntries(weight, generatedChance);
            RandomAmountConfigEntriesByAmount[amount] = entries;
            return entries;
        }

        private sealed class MoonConfigEntries(
            ConfigEntry<int> minimum,
            ConfigEntry<int> maximum,
            ConfigEntry<int> generatedChance)
        {
            public ConfigEntry<int> Minimum { get; } = minimum;
            public ConfigEntry<int> Maximum { get; } = maximum;
            public ConfigEntry<int> GeneratedChance { get; } = generatedChance;
        }

        private sealed class RandomAmountConfigEntries(
            ConfigEntry<int> weight,
            ConfigEntry<int>? generatedChance)
        {
            public ConfigEntry<int> Weight { get; } = weight;
            public ConfigEntry<int>? GeneratedChance { get; } = generatedChance;
        }
    }
}
